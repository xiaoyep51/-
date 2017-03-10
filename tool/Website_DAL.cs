using CommonHelper;
using CommonModel.ERP;
using CommonModel.Website;
using Dapper;
using CommonModel.Enum;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CommonModel;
using CommonModel.SelfAttribute;

namespace CommonDAL.DAL
{
    public class Website_DAL : SqlDbHelpDepper
    {
        private const string PROPERTY_TRAETYPE_SALE = "出售";
        private const string PROPERTY_TRAETYPE_RENT = "出租";
        private const string CUSTOMER_TRAETYPE_SALE = "求购";
        private const string CUSTOMER_TRAETYPE_RENT = "求租";

        #region 构造实体
        public DBEntity<B_DecorateType> BuildDecorateType(Reference reference, int sort)
        {
            SqlBuildTypeEnum buildType = SqlBuildTypeEnum.Insert;
            var decorateType = new B_DecorateType();
            var oldDecorateType = GetDecorateType(reference.ItemValue, false);

            if (oldDecorateType != null)
            {
                decorateType.ID = oldDecorateType.ID;
            }

            if (reference.FlagDeleted == true || reference.FlagTrashed == true)
            {
                decorateType.IsDel = 1;
                buildType = SqlBuildTypeEnum.LogicDelete;
            }
            else
            {
                if (oldDecorateType != null)
                {
                    buildType = SqlBuildTypeEnum.Update;
                }
                decorateType.IsDel = 0;
            }

            decorateType.SysCode = reference.RefID;

            if (string.IsNullOrWhiteSpace(reference.ItemValue))
            {
                LogHelper.DetailLog("装修类型导入失败，因为[{0}]类型名为空", reference.RefID);
                return null;
            }
            decorateType.Name = reference.ItemValue;
            decorateType.Sort = sort;

            //如果修改了装饰类型，其关联的房源需要修改

            return new DBEntity<B_DecorateType>(buildType, decorateType);
        }

        public DBEntity<O_Store> BuildStore(Department department)
        {
            SqlBuildTypeEnum buildType = SqlBuildTypeEnum.Insert;
            var store = new O_Store();
            var oldStore = GetStore(department.DeptID, false);
            var now = GetServerDate();

            if (oldStore != null)
            {
                store.ID = oldStore.ID;
                store.CreatDate = oldStore.CreatDate;
                store.AreaID = oldStore.AreaID;
                store.ShangQuanID = oldStore.ShangQuanID;
            }
            else
            {
                store.AreaID = 0;
                store.ShangQuanID = 0;
                store.CreatDate = now;
            }

            if (department.FlagDeleted == true || department.FlagTrashed == true)
            {
                store.IsDel = 1;
                buildType = SqlBuildTypeEnum.LogicDelete;
            }
            else
            {
                if (oldStore != null)
                {
                    buildType = SqlBuildTypeEnum.Update;
                }
                store.IsDel = 0;
            }

            store.SysCode = department.DeptID;
            store.Name = department.DeptName;
            store.StoreAddress = department.Address;
            store.Telephone = department.Tel;
            store.Longitude = department.XLocate.HasValue ? department.XLocate.Value : 0;
            store.Latitude = department.YLocate.HasValue ? department.YLocate.Value : 0;
            store.ClickNum = 0;

            var imageList = GetDepartmentImageList(department.DeptID);
            store.ImageUrl = imageList.Count != 0 ? imageList[0].ImageUrl : "";

            if (oldStore != null)
            {
                store.EsfNum = oldStore.EsfNum;
                store.CzfNum = oldStore.CzfNum;
            }
            else
            {
                store.EsfNum = 0;
                store.CzfNum = 0;
            }

            return new DBEntity<O_Store>(buildType, store);
        }

        public DBEntity<O_Agent> BuildAgent(Employee employee)
        {
            SqlBuildTypeEnum buildType = SqlBuildTypeEnum.Insert;
            var agent = new O_Agent();
            var oldAgent = GetAgent(employee.EmpID, false);
            var now = GetServerDate();

            if (oldAgent != null)
            {
                agent.ID = oldAgent.ID;
                agent.CreatDate = oldAgent.CreatDate;
            }
            else
            {
                agent.CreatDate = now;
            }

            //兼容经纪人职位变更
            if (employee.FlagDeleted == true || employee.FlagTrashed == true || employee.ZFStatus == "离职" || string.IsNullOrWhiteSpace(employee.PositionName) || !employee.PositionName.Contains("经纪人"))
            {
                if (oldAgent != null)
                {
                    agent.IsDel = 1;
                    buildType = SqlBuildTypeEnum.LogicDelete;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (oldAgent != null)
                {
                    buildType = SqlBuildTypeEnum.Update;
                }
                agent.IsDel = 0;
            }

            if (string.IsNullOrWhiteSpace(employee.mobile))
            {
                agent.Mobile = "";
            }
            else
            {
                var match = System.Text.RegularExpressions.Regex.Matches(employee.mobile, @"1[3|4|5|7|8]\d{9}");
                if (match.Count > 0)
                {
                    agent.Mobile = match[0].Value;
                }
                else
                {
                    agent.Mobile = "";
                }
            }

            agent.SysCode = employee.EmpID;
            agent.AgentName = employee.EmpName;
            agent.ImageUrl = employee.StaffImage ?? "";
            agent.WeiXinCode = employee.WeixinNo ?? "";
            agent.WeiXinImg = employee.StaffWeiXinCodeUrl ?? "";
            agent.ClickNum = 0;

            if (!employee.JoinDate.HasValue)
            {
                LogHelper.DetailLog("经纪人[{0}({1})]导入失败，JoinDate为空",
                    employee.EmpNo, employee.EmpName);
                return null;
            }
            agent.EntryTime = employee.JoinDate.Value;

            var store = GetStore(employee.DeptID);
            if (store == null)
            {
                //兼容经纪人从门店调店到非门店的情况
                if (oldAgent != null)
                {
                    agent.StoreID = 0;
                    agent.IsDel = 1;
                    buildType = SqlBuildTypeEnum.LogicDelete;
                }
                else
                {
                    LogHelper.DetailLog("经纪人[{0}({1})]导入失败，网站中没有对应的部门",
                        employee.EmpNo, employee.EmpName);
                    return null;
                }
            }
            else
            {
                agent.StoreID = store.ID;
            }

            if (oldAgent != null)
            {
                agent.EsfNum = oldAgent.EsfNum;
                agent.CzfNum = oldAgent.CzfNum;
            }
            else
            {
                agent.EsfNum = 0;
                agent.CzfNum = 0;
            }

            return new DBEntity<O_Agent>(buildType, agent);
        }

        public DBEntity<H_SaleHouse> BuildSaleHouse(Property property)
        {
            SqlBuildTypeEnum buildType = SqlBuildTypeEnum.Insert;
            var saleHouse = new H_SaleHouse();
            var oldSaleHouse = GetSaleHouse(property.PropertyID);
            var now = GetServerDate();

            if (oldSaleHouse != null)
            {
                saleHouse.ID = oldSaleHouse.ID;
                saleHouse.CreatDate = oldSaleHouse.CreatDate;
            }
            else
            {
                saleHouse.CreatDate = now;
            }

            //HouseState（0：在售 1：已下架 2：已成交 3：已下定）
            switch (property.Status)
            {
                case "有效":
                    if (property.IsOrder == null || property.IsOrder == "未下定")
                    {
                        saleHouse.HouseState = 0;
                    }
                    else if (property.IsOrder == "已下定")
                    {
                        saleHouse.HouseState = 3;
                    }
                    break;
                case "我售":
                case "他售":
                case "已售":
                case "我租":
                case "他租":
                case "已租":
                    saleHouse.HouseState = 2;
                    break;
                case "待转":
                case "预定":
                case "无效":
                case "暂缓":
                case "未知":
                default:
                    saleHouse.HouseState = 1;
                    break;
            }

            if (property.FlagDeleted == true || property.FlagTrashed == true)
            {
                if (oldSaleHouse != null)
                {
                    saleHouse.HouseState = 1;
                    buildType = SqlBuildTypeEnum.LogicDelete;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (oldSaleHouse != null)
                {
                    buildType = SqlBuildTypeEnum.Update;
                }
            }

            if (!property.CheckTimes.HasValue || property.CheckTimes == 0)
            {
                if (oldSaleHouse != null)
                {
                    saleHouse.HouseState = 1;
                    buildType = SqlBuildTypeEnum.LogicDelete;
                }
                else
                {
                    LogHelper.DetailLog("房源[{0}]导入失败，该房源未检查过或检查为无效", property.PropertyNo);
                    return null;
                }
            }

            if ((!property.ExclusivePhotoCount.HasValue || property.ExclusivePhotoCount == 0) && (!property.EntrustPhotoCount.HasValue || property.EntrustPhotoCount == 0))
            {
                if (oldSaleHouse != null)
                {
                    saleHouse.HouseState = 1;
                    buildType = SqlBuildTypeEnum.LogicDelete;
                }
                else
                {
                    LogHelper.DetailLog("房源[{0}]导入失败，该房源没有委托", property.PropertyNo);
                    return null;
                }
            }

            saleHouse.SysCode = property.PropertyID;
            saleHouse.Title = property.Title ?? "";
            saleHouse.Orientation = property.PropertyDirection ?? "";
            saleHouse.MarkName = property.TagDescription ?? "";
            saleHouse.ClickNum = 0;
            saleHouse.FollowNum = 0;
            saleHouse.LastEditDate = now;
            saleHouse.CountT = RegexHelper.TryGetNumber(property.CountT);
            saleHouse.CountF = RegexHelper.TryGetNumber(property.CountF);
            saleHouse.CountW = RegexHelper.TryGetNumber(property.CountW);
            saleHouse.SumFloor = property.FloorAll.HasValue ? property.FloorAll.Value : 0;
            saleHouse.FloorNum = property.Floor.HasValue ? property.Floor.Value : 0;

            try
            {
                saleHouse.ProducingArea = Convert.ToDecimal(property.Square);

                if (property.UnitName == "万元")
                {
                    saleHouse.Price = Convert.ToDecimal(property.Price);
                }
                else if (property.UnitName == "元")
                {
                    saleHouse.Price = Convert.ToDecimal(property.Price / 10000);
                }
                else
                {
                    if (oldSaleHouse != null)
                    {
                        saleHouse.HouseState = 1;
                        buildType = SqlBuildTypeEnum.LogicDelete;
                    }
                    else
                    {
                        LogHelper.DetailLog("房源[{0}]导入失败，无法识别该房源价格单位", property.PropertyNo);
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                if (oldSaleHouse != null)
                {
                    saleHouse.HouseState = 1;
                    buildType = SqlBuildTypeEnum.LogicDelete;
                }
                else
                {
                    LogHelper.DetailLog("房源[{0}]导入失败，部分字段转换类型时异常。{1}",
                        property.PropertyNo, e.Message);
                    return null;
                }
            }

            var buliding = GetBuilding(property.EstateID);
            if (buliding == null)
            {
                if (oldSaleHouse != null)
                {
                    saleHouse.HouseState = 1;
                    buildType = SqlBuildTypeEnum.LogicDelete;
                }
                else
                {
                    LogHelper.DetailLog("房源[{0}]导入失败，网站中没有对应的楼盘", property.PropertyNo);
                    return null;
                }
            }
            else
            {
                saleHouse.BuildingID = buliding.BuildingCode;
            }

            var priceRange = GetPriceRange(saleHouse.Price, 2);
            if (priceRange == null)
            {
                LogHelper.DetailLog("房源[{0}]导入失败，网站中没有对应的价格范围", property.PropertyNo);
                return null;
            }
            saleHouse.PriceRangeID = priceRange.ID;

            var imageList = GetPropertyImageUrlList(property.PropertyID).Select(i => i.ImageUrl).Distinct().ToList();
            if (imageList.Count == 0)
            {
                if (oldSaleHouse != null)
                {
                    saleHouse.HouseState = 1;
                    buildType = SqlBuildTypeEnum.LogicDelete;
                }
                else
                {
                    LogHelper.DetailLog("房源[{0}]导入失败，该房源没有房勘图片", property.PropertyNo);
                    return null;
                }
            }
            else
            {
                saleHouse.ImageUrl = imageList[0];
                saleHouse.ImgNum = imageList.Count;
            }

            var areaRange = GetAreaRange(property.Square.Value);
            if (areaRange == null)
            {
                LogHelper.DetailLog("房源[{0}]导入失败，网站中没有对应的面积范围", property.PropertyNo);
                return null;
            }
            saleHouse.AreaRangeID = areaRange.ID;

            if (string.IsNullOrWhiteSpace(property.PropertyDecoration))
            {
                LogHelper.DetailLog("房源[{0}]导入失败，该房源装饰类型为空", property.PropertyNo);
                return null;
            }
            var decorateType = GetDecorateType(property.PropertyDecoration);
            if (decorateType == null)
            {
                LogHelper.DetailLog("房源[{0}]导入失败，网站中没有对应的装饰类型", property.PropertyNo);
                return null;
            }
            saleHouse.DecorateTypeID = decorateType.ID;

            var agent = GetAgent(property.EmpID1);
            if (oldSaleHouse != null)
            {
                if (agent != null)
                {
                    saleHouse.AgentID = agent.ID;
                }
                else
                {
                    saleHouse.AgentID = oldSaleHouse.AgentID;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(property.EmpID1))
                {
                    LogHelper.DetailLog("房源[{0}]导入失败，该房源维护人为空", property.PropertyNo);
                    return null;
                }
                if (agent == null)
                {
                    LogHelper.DetailLog("房源[{0}]导入失败，网站中没有对应的经纪人", property.PropertyNo);
                    return null;
                }
                saleHouse.AgentID = agent.ID;
            }

            switch (property.PropertyCommission)
            {
                case "A类":
                    saleHouse.IsRecommend = 1;
                    break;
                case "B类":
                case "C类":
                case "D类":
                    saleHouse.IsRecommend = 0;
                    break;
                default:
                    saleHouse.IsRecommend = 0;
                    //LogHelper.DetailLog("房源[{0}]导入失败，无法识别的房屋类型", property.PropertyNo);
                    //return null;
                    break;
            }

            switch (property.PropertyUsage)
            {
                case "住宅":
                    saleHouse.PurposeType = 1;
                    break;
                case "别墅":
                    saleHouse.PurposeType = 2;
                    break;
                case "商铺":
                    saleHouse.PurposeType = 3;
                    break;
                case "写字楼":
                    saleHouse.PurposeType = 4;
                    break;
                default:
                    saleHouse.PurposeType = 5;
                    break;
            }

            return new DBEntity<H_SaleHouse>(buildType, saleHouse);
        }

        public DBEntity<H_RentalHouse> BuildRentalHouse(Property property)
        {
            SqlBuildTypeEnum buildType = SqlBuildTypeEnum.Insert;
            var rentalHouse = new H_RentalHouse();
            var oldRentalHouse = GetRentalHouse(property.PropertyID);
            var now = GetServerDate();

            if (oldRentalHouse != null)
            {
                rentalHouse.ID = oldRentalHouse.ID;
                rentalHouse.CreatDate = oldRentalHouse.CreatDate;
            }
            else
            {
                rentalHouse.CreatDate = now;
            }

            //HouseState（0:在租 1:已租 2:下架）
            switch (property.Status)
            {
                case "有效":
                    rentalHouse.HouseState = 0;
                    break;
                case "我售":
                case "他售":
                case "已售":
                case "我租":
                case "他租":
                case "已租":
                    rentalHouse.HouseState = 1;
                    break;
                case "待转":
                case "预定":
                case "无效":
                case "暂缓":
                case "未知":
                default:
                    rentalHouse.HouseState = 2;
                    break;
            }

            if (property.FlagDeleted == true || property.FlagTrashed == true)
            {
                if (oldRentalHouse != null)
                {
                    rentalHouse.HouseState = 2;
                    buildType = SqlBuildTypeEnum.LogicDelete;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (oldRentalHouse != null)
                {
                    buildType = SqlBuildTypeEnum.Update;
                }
            }

            //出租房检查先去掉
            //if (!property.CheckTimes.HasValue || property.CheckTimes == 0)
            //{
            //    if (oldRentalHouse != null)
            //    {
            //        rentalHouse.HouseState = 2;
            //        buildType = SqlBuildTypeEnum.LogicDelete;
            //    }
            //    else
            //    {
            //        LogHelper.DetailLog("房源[{0}]导入失败，该房源未检查过或检查为无效", property.PropertyNo);
            //        return null;
            //    }
            //}

            if ((!property.ExclusivePhotoCount.HasValue || property.ExclusivePhotoCount == 0) && (!property.EntrustPhotoCount.HasValue || property.EntrustPhotoCount == 0))
            {
                if (oldRentalHouse != null)
                {
                    rentalHouse.HouseState = 2;
                    buildType = SqlBuildTypeEnum.LogicDelete;
                }
                else
                {
                    LogHelper.DetailLog("房源[{0}]导入失败，该房源没有委托", property.PropertyNo);
                    return null;
                }
            }

            switch (property.RentType) //( 1:整租 2:合租) 0 不限
            {
                case "整租": rentalHouse.RentalMode = 1; break;
                case "合租": rentalHouse.RentalMode = 2; break;
                case "不限": rentalHouse.RentalMode = 0; break;
                default: rentalHouse.RentalMode = 0; break;
            }

            rentalHouse.SysCode = property.PropertyID;
            rentalHouse.Title = property.Title ?? "";
            rentalHouse.Orientation = property.PropertyDirection ?? "";
            rentalHouse.ClickNum = 0;
            rentalHouse.FollowNum = 0;
            rentalHouse.LastEditDate = now;
            rentalHouse.Payment = ""; //一次性、按揭、公积金
            rentalHouse.CountF = RegexHelper.TryGetNumber(property.CountF);
            rentalHouse.CountT = RegexHelper.TryGetNumber(property.CountT);
            rentalHouse.CountW = RegexHelper.TryGetNumber(property.CountW);
            rentalHouse.SumFloor = property.FloorAll.HasValue ? property.FloorAll.Value : 0;
            rentalHouse.FloorNum = property.Floor.HasValue ? property.Floor.Value : 0;

            try
            {
                rentalHouse.ProducingArea = Convert.ToDecimal(property.Square);

                if (property.RentUnitName == "元/月")
                {
                    rentalHouse.Price = Convert.ToDecimal(property.RentPrice);
                }
                else
                {
                    if (oldRentalHouse != null)
                    {
                        rentalHouse.HouseState = 2;
                        buildType = SqlBuildTypeEnum.LogicDelete;
                    }
                    else
                    {
                        LogHelper.DetailLog("房源[{0}]导入失败，无法识别该房源价格单位", property.PropertyNo);
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                if (oldRentalHouse != null)
                {
                    rentalHouse.HouseState = 2;
                    buildType = SqlBuildTypeEnum.LogicDelete;
                }
                else
                {
                    LogHelper.DetailLog("房源[{0}]导入失败，部分字段转换类型时异常。{1}",
                        property.PropertyNo, e.Message);
                    return null;
                }
            }

            var buliding = GetBuilding(property.EstateID);
            if (buliding == null)
            {
                if (oldRentalHouse != null)
                {
                    rentalHouse.HouseState = 2;
                    buildType = SqlBuildTypeEnum.LogicDelete;
                }
                else
                {
                    LogHelper.DetailLog("房源[{0}]导入失败，网站中没有对应的楼盘", property.PropertyNo);
                    return null;
                }
            }
            else
            {
                rentalHouse.BuildingID = buliding.BuildingCode;
            }

            var priceRange = GetPriceRange(rentalHouse.Price, 3);
            if (priceRange == null)
            {
                LogHelper.DetailLog("房源[{0}]导入失败，网站中没有对应的价格范围", property.PropertyNo);
                return null;
            }
            rentalHouse.PriceRangeID = priceRange.ID;

            var imageList = GetPropertyImageUrlList(property.PropertyID).Select(i => i.ImageUrl).Distinct().ToList();
            if (imageList.Count == 0)
            {
                if (oldRentalHouse != null)
                {
                    rentalHouse.HouseState = 2;
                    buildType = SqlBuildTypeEnum.LogicDelete;
                }
                else
                {
                    LogHelper.DetailLog("房源[{0}]导入失败，该房源没有房勘图片", property.PropertyNo);
                    return null;
                }
            }
            else
            {
                rentalHouse.ImageUrl = imageList[0];
                rentalHouse.ImgNum = imageList.Count;
            }

            var areaRange = GetAreaRange(property.Square.Value);
            if (areaRange == null)
            {
                LogHelper.DetailLog("房源[{0}]导入失败，网站中没有对应的面积范围", property.PropertyNo);
                return null;
            }
            rentalHouse.AreaRangeID = areaRange.ID;

            if (string.IsNullOrWhiteSpace(property.PropertyDecoration))
            {
                LogHelper.DetailLog("房源[{0}]导入失败，该房源装饰类型为空", property.PropertyNo);
                return null;
            }
            var decorateType = GetDecorateType(property.PropertyDecoration);
            if (decorateType == null)
            {
                LogHelper.DetailLog("房源[{0}]导入失败，网站中没有对应的装饰类型", property.PropertyNo);
                return null;
            }
            rentalHouse.DecorateTypeID = decorateType.ID;
            rentalHouse.MarkName = property.TagDescription ?? "";

            var agent = GetAgent(property.EmpID1);
            if (oldRentalHouse != null)
            {
                if (agent != null)
                {
                    rentalHouse.AgentID = agent.ID;
                }
                else
                {
                    rentalHouse.AgentID = oldRentalHouse.AgentID;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(property.EmpID1))
                {
                    LogHelper.DetailLog("房源[{0}]导入失败，该房源维护人为空", property.PropertyNo);
                    return null;
                }
                if (agent == null)
                {
                    LogHelper.DetailLog("房源[{0}]导入失败，网站中没有对应的经纪人", property.PropertyNo);
                    return null;
                }
                rentalHouse.AgentID = agent.ID;
            }

            switch (property.PropertyUsage)
            {
                case "住宅":
                    rentalHouse.PurposeType = 1;
                    break;
                case "别墅":
                    rentalHouse.PurposeType = 2;
                    break;
                case "商铺":
                    rentalHouse.PurposeType = 3;
                    break;
                case "写字楼":
                    rentalHouse.PurposeType = 4;
                    break;
                default:
                    rentalHouse.PurposeType = 5;
                    break;
            }

            return new DBEntity<H_RentalHouse>(buildType, rentalHouse);
        }

        public DBEntity<H_SaleHouseImg> BuildSaleHouseImg(ImageInfo imageInfo)
        {
            SqlBuildTypeEnum buildType = SqlBuildTypeEnum.Insert;
            var saleHouseImg = new H_SaleHouseImg();
            var saleHouse = GetSaleHouse(imageInfo.PropertyID);
            if (saleHouse == null)
            {
                LogHelper.DetailLog("出售房源图片导入失败，网站中没有对应的出售房源[{0}]", imageInfo.PropertyID);
                return null;
            }
            var oldSaleHouseImg = GetSaleHouseImg(saleHouse.ID, imageInfo.ImageUrl);
            var now = GetServerDate();

            if (oldSaleHouseImg != null)
            {
                saleHouseImg.ID = oldSaleHouseImg.ID;
                saleHouseImg.CreatDate = oldSaleHouseImg.CreatDate;
            }
            else
            {
                saleHouseImg.CreatDate = now;
            }

            if (imageInfo.FlagDelete == true || imageInfo.FlagTrashed == true)
            {
                buildType = SqlBuildTypeEnum.RealDelete;
            }
            else
            {
                if (oldSaleHouseImg != null)
                {
                    buildType = SqlBuildTypeEnum.Update;
                }
            }

            saleHouseImg.SaleHouseID = saleHouse.ID;
            saleHouseImg.ImageUrl = imageInfo.ImageUrl;
            var isTitleImg = saleHouse.ImageUrl == imageInfo.ImageUrl;
            saleHouseImg.IsTitleImg = Convert.ToByte(isTitleImg);

            return new DBEntity<H_SaleHouseImg>(buildType, saleHouseImg);
        }

        public DBEntity<H_RentalHouseImg> BuildRentalHouseImg(ImageInfo imageInfo)
        {
            SqlBuildTypeEnum buildType = SqlBuildTypeEnum.Insert;
            var rentalHouseImg = new H_RentalHouseImg();
            var rentalHouse = GetRentalHouse(imageInfo.PropertyID);
            if (rentalHouse == null)
            {
                LogHelper.DetailLog("出租房源图片导入失败，网站中没有对应的出租房源[{0}]", imageInfo.PropertyID);
                return null;
            }
            var oldRentalHouseImg = GetRentalHouseImg(rentalHouse.ID, imageInfo.ImageUrl);
            var now = GetServerDate();

            if (oldRentalHouseImg != null)
            {
                rentalHouseImg.ID = oldRentalHouseImg.ID;
                rentalHouseImg.CreatDate = oldRentalHouseImg.CreatDate;
            }
            else
            {
                rentalHouseImg.CreatDate = now;
            }

            if (imageInfo.FlagDelete == true || imageInfo.FlagTrashed == true)
            {
                buildType = SqlBuildTypeEnum.RealDelete;
            }
            else
            {
                if (oldRentalHouseImg != null)
                {
                    buildType = SqlBuildTypeEnum.Update;
                }
            }

            rentalHouseImg.RentalHouseID = rentalHouse.ID;
            rentalHouseImg.ImageUrl = imageInfo.ImageUrl;
            var isTitleImg = rentalHouse.ImageUrl == imageInfo.ImageUrl;
            rentalHouseImg.IsTitleImg = Convert.ToByte(isTitleImg);

            return new DBEntity<H_RentalHouseImg>(buildType, rentalHouseImg);
        }

        public DBEntity<H_SaleHouseEvaluate> BuildSaleHouseEvaluate(PropertyComment propertyComment)
        {
            SqlBuildTypeEnum buildType = SqlBuildTypeEnum.Insert;
            var saleHouseEvaluate = new H_SaleHouseEvaluate();
            var oldSaleHouseEvaluate = GetSaleHouseEvaluate(propertyComment.CommentID);
            var now = GetServerDate();

            if (oldSaleHouseEvaluate != null)
            {
                saleHouseEvaluate.ID = oldSaleHouseEvaluate.ID;
                saleHouseEvaluate.CreatDate = oldSaleHouseEvaluate.CreatDate;
            }
            else
            {
                saleHouseEvaluate.CreatDate = now;
            }

            if (propertyComment.FlagDeleted == true || propertyComment.FlagTrashed == true)
            {
                buildType = SqlBuildTypeEnum.RealDelete;
            }
            else
            {
                if (oldSaleHouseEvaluate != null)
                {
                    buildType = SqlBuildTypeEnum.Update;
                }
            }

            saleHouseEvaluate.SysCode = propertyComment.CommentID;
            var saleHouse = GetSaleHouse(propertyComment.PropertyID);
            if (saleHouse == null)
            {
                LogHelper.DetailLog("出售房源评价[{0}]导入失败，网站中没有对应的出售房源[{1}]", propertyComment.CommentID, propertyComment.PropertyID);
                return null;
            }
            saleHouseEvaluate.SaleHouseID = saleHouse.ID;
            var agent = GetAgent(propertyComment.CommentEmpID);
            if (agent == null)
            {
                LogHelper.DetailLog("出售房源评价[{0}]导入失败，网站中没有对应的经纪人[{1}]", propertyComment.CommentID, propertyComment.CommentEmpID);
                return null;
            }
            saleHouseEvaluate.AgentID = agent.ID;
            saleHouseEvaluate.Contents = propertyComment.CommentContent;
            if (!propertyComment.CommentDate.HasValue)
            {
                LogHelper.DetailLog("出售房源评价[{0}]导入失败，评价时间为空", propertyComment.CommentID);
                return null;
            }
            saleHouseEvaluate.EvaluateDate = propertyComment.CommentDate.Value;

            return new DBEntity<H_SaleHouseEvaluate>(buildType, saleHouseEvaluate);
        }

        public DBEntity<H_RentalHouseEvaluate> BuildRentalHouseEvaluate(PropertyComment propertyComment)
        {
            SqlBuildTypeEnum buildType = SqlBuildTypeEnum.Insert;
            var rentalHouseEvaluate = new H_RentalHouseEvaluate();
            var oldRentalHouseEvaluate = GetRentalHouseEvaluate(propertyComment.CommentID);
            var now = GetServerDate();

            if (oldRentalHouseEvaluate != null)
            {
                rentalHouseEvaluate.ID = oldRentalHouseEvaluate.ID;
                rentalHouseEvaluate.CreatDate = oldRentalHouseEvaluate.CreatDate;
            }
            else
            {
                rentalHouseEvaluate.CreatDate = now;
            }

            if (propertyComment.FlagDeleted == true || propertyComment.FlagTrashed == true)
            {
                buildType = SqlBuildTypeEnum.RealDelete;
            }
            else
            {
                if (oldRentalHouseEvaluate != null)
                {
                    buildType = SqlBuildTypeEnum.Update;
                }
            }

            rentalHouseEvaluate.SysCode = propertyComment.CommentID;
            var rentalHouse = GetRentalHouse(propertyComment.PropertyID);
            if (rentalHouse == null)
            {
                LogHelper.DetailLog("出租房源评价[{0}]导入失败，网站中没有对应的出租房源[{1}]", propertyComment.CommentID, propertyComment.PropertyID);
                return null;
            }
            rentalHouseEvaluate.RentalHouseID = rentalHouse.ID;
            var agent = GetAgent(propertyComment.CommentEmpID);
            if (agent == null)
            {
                LogHelper.DetailLog("出售房源评价[{0}]导入失败，网站中没有对应的经纪人[{1}]", propertyComment.CommentID, propertyComment.CommentEmpID);
                return null;
            }
            rentalHouseEvaluate.AgentID = agent.ID;
            rentalHouseEvaluate.Contents = propertyComment.CommentContent;
            if (!propertyComment.CommentDate.HasValue)
            {
                LogHelper.DetailLog("出租房源评价[{0}]导入失败，评价时间为空", propertyComment.CommentID);
                return null;
            }
            rentalHouseEvaluate.EvaluateDate = propertyComment.CommentDate.Value;

            return new DBEntity<H_RentalHouseEvaluate>(buildType, rentalHouseEvaluate);
        }

        public DBEntity<H_SaleHouseSeeLog> BuildSaleHouseSeeLog(BeltWatch beltWatch, Property property, Inquiry inquiry)
        {
            var tripleDESCryptoHelper = new TripleDESCryptoHelper(MobileEncrypKey.KEY, MobileEncrypKey.IV);
            SqlBuildTypeEnum buildType = SqlBuildTypeEnum.Insert;
            var saleHouseSeeLog = new H_SaleHouseSeeLog();
            var now = GetServerDate();

            var saleHouse = GetSaleHouse(property.PropertyID);
            if (saleHouse == null)
            {
                LogHelper.DetailLog("带看[{0}]导入失败，网站中没有对应出售房源[{1}]", beltWatch.BeltWatchID, beltWatch.PropertyNo);
                return null;
            }
            var oldSaleHouseSeeLog = GetSaleHouseSeeLog(beltWatch.BeltWatchGroupID, saleHouse.ID);

            if (oldSaleHouseSeeLog != null)
            {
                saleHouseSeeLog.ID = oldSaleHouseSeeLog.ID;
                saleHouseSeeLog.CreatDate = oldSaleHouseSeeLog.CreatDate;
            }
            else
            {
                saleHouseSeeLog.CreatDate = now;
            }

            //StatusInt 0删除，1有效
            if (beltWatch.StatusInt == 0)
            {
                buildType = SqlBuildTypeEnum.RealDelete;
            }
            else
            {
                if (oldSaleHouseSeeLog != null)
                {
                    buildType = SqlBuildTypeEnum.Update;
                }
            }

            if (string.IsNullOrWhiteSpace(beltWatch.BeltWatchGroupID))
            {
                LogHelper.DetailLog("带看[{0}]导入失败，该带看组ID为空", beltWatch.BeltWatchID);
                return null;
            }
            saleHouseSeeLog.SysCode = beltWatch.BeltWatchGroupID;
            saleHouseSeeLog.SysClientCode = inquiry.InquiryID;
            if (oldSaleHouseSeeLog != null)
            {
                saleHouseSeeLog.IsEvaluate = oldSaleHouseSeeLog.IsEvaluate;
            }
            else
            {
                saleHouseSeeLog.IsEvaluate = 0;
            }
            //加密客户手机
            if (string.IsNullOrWhiteSpace(inquiry.CustMobile))
            {
                saleHouseSeeLog.UsersMobile = "";
            }
            else
            {
                var match = System.Text.RegularExpressions.Regex.Matches(inquiry.CustMobile, @"1[3|4|5|7|8]\d{9}");
                if (match.Count > 0)
                {
                    var encrypt = tripleDESCryptoHelper.Encrypt(match[0].Value);
                    saleHouseSeeLog.UsersMobile = encrypt;
                }
                else
                {
                    saleHouseSeeLog.UsersMobile = "";
                }
            }
            if (!beltWatch.BeltWatchDate.HasValue)
            {
                LogHelper.DetailLog("带看[{0}]导入失败，带看时间为空", beltWatch.BeltWatchID);
                return null;
            }
            saleHouseSeeLog.SeeDate = beltWatch.BeltWatchDate.Value;
            saleHouseSeeLog.SaleHouseID = saleHouse.ID;

            var agent = GetAgent(beltWatch.EmpID);
            if (agent == null)
            {
                LogHelper.DetailLog("带看[{0}]导入失败，网站中没有对应的经纪人[{1}]", beltWatch.BeltWatchID, beltWatch.EmpID);
                return null;
            }
            saleHouseSeeLog.AgentID = agent.ID;
            saleHouseSeeLog.AgentName = agent.AgentName;
            saleHouseSeeLog.AgentMobile = agent.Mobile;

            return new DBEntity<H_SaleHouseSeeLog>(buildType, saleHouseSeeLog);
        }

        public DBEntity<H_RentalHouseSeeLog> BuildRentalHouseSeeLog(BeltWatch beltWatch, Property property, Inquiry inquiry)
        {
            var tripleDESCryptoHelper = new TripleDESCryptoHelper(MobileEncrypKey.KEY, MobileEncrypKey.IV);
            SqlBuildTypeEnum buildType = SqlBuildTypeEnum.Insert;
            var rentalHouseSeeLog = new H_RentalHouseSeeLog();
            var now = GetServerDate();

            var rentalHouse = GetRentalHouse(property.PropertyID);
            if (rentalHouse == null)
            {
                LogHelper.DetailLog("带看[{0}]导入失败，网站中没有对应出租房源[{1}]", beltWatch.BeltWatchID, beltWatch.PropertyNo);
                return null;
            }
            var oldRentalHouseSeeLog = GetRentalHouseSeeLog(beltWatch.BeltWatchGroupID, rentalHouse.ID);

            if (oldRentalHouseSeeLog != null)
            {
                rentalHouseSeeLog.ID = oldRentalHouseSeeLog.ID;
                rentalHouseSeeLog.CreatDate = oldRentalHouseSeeLog.CreatDate;
            }
            else
            {
                rentalHouseSeeLog.CreatDate = now;
            }

            //StatusInt 0删除，1有效
            if (beltWatch.StatusInt == 0)
            {
                buildType = SqlBuildTypeEnum.RealDelete;
            }
            else
            {
                if (oldRentalHouseSeeLog != null)
                {
                    buildType = SqlBuildTypeEnum.Update;
                }
            }

            if (string.IsNullOrWhiteSpace(beltWatch.BeltWatchGroupID))
            {
                LogHelper.DetailLog("带看[{0}]导入失败，该带看组ID为空", beltWatch.BeltWatchID);
                return null;
            }
            rentalHouseSeeLog.SysCode = beltWatch.BeltWatchGroupID;
            rentalHouseSeeLog.SysClientCode = inquiry.InquiryID;
            if (oldRentalHouseSeeLog != null)
            {
                rentalHouseSeeLog.IsEvaluate = oldRentalHouseSeeLog.IsEvaluate;
            }
            else
            {
                rentalHouseSeeLog.IsEvaluate = 0;
            }
            //加密客户手机
            if (string.IsNullOrWhiteSpace(inquiry.CustMobile))
            {
                rentalHouseSeeLog.UsersMobile = "";
            }
            else
            {
                var match = System.Text.RegularExpressions.Regex.Matches(inquiry.CustMobile, @"1[3|4|5|7|8]\d{9}");
                if (match.Count > 0)
                {
                    var encrypt = tripleDESCryptoHelper.Encrypt(match[0].Value);
                    rentalHouseSeeLog.UsersMobile = encrypt;
                }
                else
                {
                    rentalHouseSeeLog.UsersMobile = "";
                }
            }
            if (!beltWatch.BeltWatchDate.HasValue)
            {
                LogHelper.DetailLog("带看[{0}]导入失败，带看时间为空", beltWatch.BeltWatchID);
                return null;
            }
            rentalHouseSeeLog.SeeDate = beltWatch.BeltWatchDate.Value;
            rentalHouseSeeLog.RentalHouseID = rentalHouse.ID;

            var agent = GetAgent(beltWatch.EmpID);
            if (agent == null)
            {
                LogHelper.DetailLog("带看[{0}]导入失败，网站中没有对应的经纪人[{1}]", beltWatch.BeltWatchID, beltWatch.EmpID);
                return null;
            }
            rentalHouseSeeLog.AgentID = agent.ID;
            rentalHouseSeeLog.AgentName = agent.AgentName;
            rentalHouseSeeLog.AgentMobile = agent.Mobile;

            return new DBEntity<H_RentalHouseSeeLog>(buildType, rentalHouseSeeLog);
        }

        public DBEntity<S_ContractInfo> BuildContractInfo(Contract contract, bool isRegisterAPI = false, string fileNameExtend = "")
        {
            var tripleDESCryptoHelper = new TripleDESCryptoHelper(MobileEncrypKey.KEY, MobileEncrypKey.IV);
            SqlBuildTypeEnum buildType = SqlBuildTypeEnum.Insert;
            var contractInfo = new S_ContractInfo();

            var saleHouse = GetSaleHouse(contract.PropertyID);
            var rentalHouse = GetRentalHouse(contract.PropertyID);
            switch (contract.Trade)
            {
                case "出售":
                case "代办":
                    if (saleHouse == null)
                    {
                        LogHelper.DetailLog("合同[{0}]导入失败，网站中没有对应房源[{1}]", contract.ContractNo, contract.PropertyNo);
                        return null;
                    }
                    break;
                case "出租":
                    if (rentalHouse == null)
                    {
                        LogHelper.DetailLog("合同[{0}]导入失败，网站中没有对应房源[{1}]", contract.ContractNo, contract.PropertyNo);
                        return null;
                    }
                    break;
                default:
                    LogHelper.DetailLog("合同[{0}]导入失败，无法识别的交易类型[{1}]", contract.ContractNo, contract.PropertyNo);
                    return null;
            }

            var contractReport = GetContractReportByNo(contract.ContractNo);
            //if (contractReport == null)
            //{
            //    if (isRegisterAPI)
            //    {
            //        LogHelper.APILog(fileNameExtend, "合同[{0}]导入失败，ERP系统中没有对应合同记录", contract.ContractNo);
            //    }
            //    else
            //    {
            //        LogHelper.DetailLog("合同[{0}]导入失败，ERP系统中没有对应合同记录", contract.ContractNo);
            //    }
            //    return null;
            //}
            var oldContractInfo = GetContractInfo(contract.ContractNo);
            var now = GetServerDate();

            if (oldContractInfo != null)
            {
                contractInfo.ID = oldContractInfo.ID;
                contractInfo.CreateDate = oldContractInfo.CreateDate;
            }
            else
            {
                contractInfo.CreateDate = now;
            }

            if (contract.FlagDeleted == true || contract.FlagTrashed == true)
            {
                buildType = SqlBuildTypeEnum.RealDelete;
            }
            else
            {
                if (oldContractInfo != null)
                {
                    buildType = SqlBuildTypeEnum.Update;
                }
            }

            var property = GetPropertyByID(contract.PropertyID);
            if (property == null)
            {
                if (isRegisterAPI)
                {
                    LogHelper.APILog(fileNameExtend, "合同[{0}]导入失败，ERP系统中没有对应房源[{1}]", contract.ContractNo, contract.PropertyNo);
                }
                else
                {
                    LogHelper.DetailLog("合同[{0}]导入失败，ERP系统中没有对应房源[{1}]", contract.ContractNo, contract.PropertyNo);
                }
                return null;
            }
            contractInfo.Layout = string.Format("{0}室{1}厅",
                string.IsNullOrWhiteSpace(property.CountF) ? "0" : property.CountF,
                string.IsNullOrWhiteSpace(property.CountT) ? "0" : property.CountT);

            if (!contract.ContractDate.HasValue)
            {
                if (isRegisterAPI)
                {
                    LogHelper.APILog(fileNameExtend, "合同[{0}]导入失败，该合同的合同时间不存在", contract.ContractNo);
                }
                else
                {
                    LogHelper.DetailLog("合同[{0}]导入失败，该合同的合同时间不存在", contract.ContractNo);
                }
                return null;
            }
            contractInfo.ContractDate = contract.ContractDate.Value;
            if (!contract.Price.HasValue)
            {
                if (isRegisterAPI)
                {
                    LogHelper.APILog(fileNameExtend, "合同[{0}]导入失败，该合同的价格为空", contract.ContractNo);
                }
                else
                {
                    LogHelper.DetailLog("合同[{0}]导入失败，该合同的价格为空", contract.ContractNo);
                }
                return null;
            }
            contractInfo.Price = contract.Price.Value;
            if (!contract.Square.HasValue)
            {
                if (isRegisterAPI)
                {
                    LogHelper.APILog(fileNameExtend, "合同[{0}]导入失败，该合同的面积为空", contract.ContractNo);
                }
                else
                {
                    LogHelper.DetailLog("合同[{0}]导入失败，该合同的面积为空", contract.ContractNo);
                }
                return null;
            }
            contractInfo.Square = contract.Square.Value;

            //加密客户手机
            if (string.IsNullOrWhiteSpace(contract.OwnerMobile))
            {
                contractInfo.OwnerMobile = "";
            }
            else
            {
                var match = System.Text.RegularExpressions.Regex.Matches(contract.OwnerMobile, @"1[3|4|5|7|8]\d{9}");
                if (match.Count > 0)
                {
                    var encrypt = tripleDESCryptoHelper.Encrypt(match[0].Value);
                    contractInfo.OwnerMobile = encrypt;
                }
                else
                {
                    contractInfo.OwnerMobile = "";
                }
            }

            if (string.IsNullOrWhiteSpace(contract.CustMobile))
            {
                contractInfo.CustMobile = "";
            }
            else
            {
                var match = System.Text.RegularExpressions.Regex.Matches(contract.CustMobile, @"1[3|4|5|7|8]\d{9}");
                if (match.Count > 0)
                {
                    var encrypt = tripleDESCryptoHelper.Encrypt(match[0].Value);
                    contractInfo.CustMobile = encrypt;
                }
                else
                {
                    contractInfo.CustMobile = "";
                }
            }

            contractInfo.SysCode = contractReport != null ? contractReport.ContractReportID : "";
            contractInfo.ContractNo = contract.ContractNo;
            contractInfo.Trade = contract.Trade;
            contractInfo.PropertyID = contract.PropertyID;
            contractInfo.OwnerName = contract.OwnerName;
            contractInfo.InquiryID = contract.InquiryID;
            contractInfo.Address = contract.Address ?? "";
            contractInfo.OwnerMaritalStatus = contract.OwnerCountry.Length > 10 ? contract.OwnerCountry.Substring(0, 10) : contract.OwnerCountry;
            contractInfo.OwnerCard = contract.OwnerCard;
            contractInfo.CustName = contract.CustName;
            contractInfo.CustMaritalStatus = contract.CustCountry.Length > 10 ? contract.CustCountry.Substring(0, 10) : contract.CustCountry;
            contractInfo.CustCard = contract.CustCard;
            contractInfo.RealEstateBureau = contract.RealEstateBureau;
            contractInfo.TwoCard = contract.TwoCard;
            contractInfo.EarlyRepayment = contract.EarlyRepayment;
            contractInfo.OwnerNumber = contract.OwnerNumber;
            contractInfo.CustPaymentMode = contract.CustPaymentMode;
            contractInfo.CustNumber = contract.CustNumber;
            contractInfo.CustLoans = contract.CustLoans;
            contractInfo.CustLoansNumber = contract.CustLoansNumber == null ? "" : (contract.CustLoansNumber.Length > 10 ? contract.CustLoansNumber.Substring(0, 10) : contract.CustLoansNumber);
            contractInfo.CertificateNo = contract.CertificateNo;
            contractInfo.LandCertificateNo = contractReport != null ? contractReport.LandCertificateNo : "";
            contractInfo.FlagLoanSelf = Convert.ToByte(contract.FlagLoanSelf);
            contractInfo.EvaCompany = contract.EvaCompany;
            contractInfo.MortBank = contract.MortBank;
            contractInfo.MortMoney = contract.MortMoney.HasValue ? contract.MortMoney.Value : 0;
            contractInfo.MortYears = contract.MortYears.HasValue ? contract.MortYears.Value : 0;
            contractInfo.EvaMoney = contract.EvaMoney.HasValue ? contract.EvaMoney.Value : 0;
            contractInfo.MortBankG = contract.MortBankG;
            contractInfo.MortMoneyG = contract.MortMoneyG.HasValue ? contract.MortMoneyG.Value : 0;
            contractInfo.MortYearsG = contract.MortYearsG.HasValue ? contract.MortYearsG.Value : 0;
            contractInfo.Remark = contract.Remark;

            return new DBEntity<S_ContractInfo>(buildType, contractInfo);
        }

        public DBEntity<S_ContractFollow> BuildContractFollow(Flow flow, bool isRegisterAPI = false, string fileNameExtend = "")
        {
            SqlBuildTypeEnum buildType = SqlBuildTypeEnum.Insert;
            var contractFollow = new S_ContractFollow();
            var oldContractFollow = GetContractFollow(flow.FlowID);
            var now = GetServerDate();

            if (oldContractFollow != null)
            {
                contractFollow.ID = oldContractFollow.ID;
                contractFollow.CreateDate = oldContractFollow.CreateDate;
            }
            else
            {
                contractFollow.CreateDate = now;
            }

            if (flow.FlagDeleted == true || flow.FlagTrashed == true)
            {
                buildType = SqlBuildTypeEnum.RealDelete;
            }
            else
            {
                if (oldContractFollow != null)
                {
                    buildType = SqlBuildTypeEnum.Update;
                }
            }

            if (!isRegisterAPI)
            {
                var contractReport = GetContractReportByID(flow.ContractID);
                if (contractReport == null)
                {
                    LogHelper.DetailLog("合同进度[{0}]导入失败，网站中没有对应的合同[{1}]", flow.FlowID, flow.ContractID);
                    return null;
                }
                var contractInfo = GetContractInfo(contractReport.ContractNo);
                if (contractInfo == null)
                {
                    LogHelper.DetailLog("合同进度[{0}]导入失败，网站中没有对应的合同[{1}]", flow.FlowID, flow.ContractID);
                    return null;
                }
            }

            contractFollow.SysCode = flow.FlowID;
            contractFollow.ContractID = flow.ContractID;
            contractFollow.FollowName = string.Format("[{0}]{1}", flow.FlowNo, flow.FlowName);
            if (contractFollow.FollowName.Length > 32)
            {
                if (isRegisterAPI)
                {
                    LogHelper.APILog(fileNameExtend, "合同进度[{0}]导入失败，进度名称超过32位字符", flow.FlowID);
                }
                else
                {
                    LogHelper.DetailLog("合同进度[{0}]导入失败，进度名称超过32位字符", flow.FlowID);
                }
                return null;
            }

            if (!flow.PlanDate.HasValue)
            {
                if (isRegisterAPI)
                {
                    LogHelper.APILog(fileNameExtend, "合同进度[{0}]导入失败，计划完成日期为空", flow.FlowID);
                }
                else
                {
                    LogHelper.DetailLog("合同进度[{0}]导入失败，计划完成日期为空", flow.FlowID);
                }
                return null;
            }
            contractFollow.PlanDate = flow.PlanDate.Value;
            if (!flow.FinishedDate.HasValue)
            {
                if (isRegisterAPI)
                {
                    LogHelper.APILog(fileNameExtend, "合同进度[{0}]导入失败，实际完成日期为空", flow.FlowID);
                }
                else
                {
                    LogHelper.DetailLog("合同进度[{0}]导入失败，实际完成日期为空", flow.FlowID);
                }
                return null;
            }
            contractFollow.ActualDate = flow.FinishedDate.Value;

            var agent = GetAgent(flow.EmpID);
            if (agent == null)
            {
                if (isRegisterAPI)
                {
                    LogHelper.APILog(fileNameExtend, "合同进度[{0}]导入失败，网站中没有对应的经纪人[{1}]", flow.FlowID, flow.EmpID);
                }
                else
                {
                    LogHelper.DetailLog("合同进度[{0}]导入失败，网站中没有对应的经纪人[{1}]", flow.FlowID, flow.EmpID);
                }
                return null;
            }
            contractFollow.AgentID = flow.EmpID;
            contractFollow.AgentMobile = agent.Mobile;

            return new DBEntity<S_ContractFollow>(buildType, contractFollow);
        }

        public U_DemandInfo BuildDemandInfo(Inquiry inquiry, U_Users user, string fileNameExtend)
        {
            var tripleDESCryptoHelper = new TripleDESCryptoHelper(MobileEncrypKey.KEY, MobileEncrypKey.IV);
            var demandInfo = new U_DemandInfo();
            var now = GetServerDate();

            demandInfo.UsersID = user.ID;
            if (string.IsNullOrWhiteSpace(inquiry.CustName))
            {
                LogHelper.APILog(fileNameExtend, "客源[{0}]转需求失败，联系人姓名为空", inquiry.InquiryNo);
                return null;
            }
            demandInfo.Contacts = inquiry.CustName.Length > 8 ? inquiry.CustName.Substring(0, 8) : inquiry.CustName;

            if (inquiry.Trade == CUSTOMER_TRAETYPE_SALE) //DemandType(0:求租 1:求购)
            {
                demandInfo.DemandType = 1;
            }
            else if (inquiry.Trade == CUSTOMER_TRAETYPE_RENT)
            {
                demandInfo.DemandType = 0;
            }
            else
            {
                LogHelper.APILog(fileNameExtend, "客源[{0}]转需求失败，需求类型无法识别", inquiry.InquiryNo);
                return null;
            }

            var estate = GetEstate(inquiry.Position);
            D_Building buliding = null;
            if (estate != null)
            {
                buliding = GetBuilding(estate.EstateID);
            }
            if (buliding == null)
            {
                demandInfo.AreaID = 0;
                demandInfo.ShangQuan = 0;
                demandInfo.BuildingID = 0;
                demandInfo.BuildingName = "";
            }
            else
            {
                demandInfo.AreaID = buliding.AreaID;
                demandInfo.ShangQuan = buliding.ShangQuanID;
                demandInfo.BuildingID = buliding.BuildingCode;
                demandInfo.BuildingName = buliding.BuildingName;
            }

            demandInfo.BuildingAddress = ""; //ERP中没有对应的字段
            try
            {
                demandInfo.MinArea = Convert.ToInt32(inquiry.SquareMin);
                demandInfo.MaxArea = Convert.ToInt32(inquiry.SquareMax);
            }
            catch (Exception e)
            {
                LogHelper.APILog(fileNameExtend, "客源[{0}]转需求失败，部分字段转换类型时异常。{1}",
                    inquiry.InquiryNo, e.Message);
                return null;
            }

            //加密客户手机
            //var encrypt = tripleDESCryptoHelper.Encrypt(user.Mobile);
            //demandInfo.Mobile = encrypt;
            demandInfo.Mobile = user.Mobile;
            demandInfo.Contents = inquiry.Remark ?? "";
            demandInfo.SourceType = 0; //SourceType（0:pc 1:wap）
            demandInfo.IsDel = 0;
            demandInfo.CreatDate = now;
            demandInfo.IsInputSys = 1;
            demandInfo.SysCode = inquiry.InquiryID;
            demandInfo.InputDate = inquiry.RegDate.HasValue ? inquiry.RegDate.Value : now;
            demandInfo.SysAgentID = inquiry.EmpID;
            demandInfo.CountF = RegexHelper.TryGetNumber(inquiry.CountF);
            demandInfo.CountT = RegexHelper.TryGetNumber(inquiry.CountT);
            demandInfo.CountW = RegexHelper.TryGetNumber(inquiry.CountW);
            demandInfo.MinPrice = inquiry.PriceMin.HasValue ? inquiry.PriceMin.Value : 0;
            demandInfo.MaxPrice = inquiry.PriceMax.HasValue ? inquiry.PriceMax.Value : 0;

            return demandInfo;
        }

        public U_EntrustedInfo BuildEntrustedInfo(Property property, U_Users user, string fileNameExtend)
        {
            var tripleDESCryptoHelper = new TripleDESCryptoHelper(MobileEncrypKey.KEY, MobileEncrypKey.IV);
            var entrustedInfo = new U_EntrustedInfo();
            var now = GetServerDate();

            if (string.IsNullOrWhiteSpace(property.OwnerName))
            {
                LogHelper.APILog(fileNameExtend, "房源[{0}]转委托失败，联系人1姓名为空", property.PropertyNo);
                return null;
            }
            entrustedInfo.Contacts = property.OwnerName.Length > 8 ? property.OwnerName.Substring(0, 8) : property.OwnerName;

            if (property.Trade == PROPERTY_TRAETYPE_SALE) //EntrusteType(0:出租 1:出售)
            {
                entrustedInfo.EntrusteType = 1;
            }
            else if (property.Trade == PROPERTY_TRAETYPE_RENT)
            {
                entrustedInfo.EntrusteType = 0;
            }
            else
            {
                LogHelper.APILog(fileNameExtend, "房源[{0}]转委托失败，交易类型无法识别", property.PropertyNo);
                return null;
            }

            var buliding = GetBuilding(property.EstateID);
            if (buliding == null)
            {
                LogHelper.APILog(fileNameExtend, "房源[{0}]转委托失败，网站中没有对应的楼盘", property.PropertyNo);
                return null;
            }
            entrustedInfo.AreaID = buliding.AreaID;
            entrustedInfo.ShangQuanID = buliding.ShangQuanID;
            entrustedInfo.BuildingID = buliding.BuildingCode;
            entrustedInfo.BuildingName = buliding.BuildingName;
            entrustedInfo.Price = Convert.ToDecimal(property.Price);
            entrustedInfo.CountF = RegexHelper.TryGetNumber(property.CountF);
            entrustedInfo.CountT = RegexHelper.TryGetNumber(property.CountT);
            entrustedInfo.CountW = RegexHelper.TryGetNumber(property.CountW);
            entrustedInfo.SumFloor = property.FloorAll.HasValue ? property.FloorAll.Value : 0;
            entrustedInfo.FloorNum = property.Floor.HasValue ? property.Floor.Value : 0;

            entrustedInfo.BuildingAddress = buliding.XQAddress;
            try
            {
                entrustedInfo.ProducingArea = Convert.ToDecimal(property.Square);
            }
            catch (Exception e)
            {
                LogHelper.APILog(fileNameExtend, "房源[{0}]转委托失败，部分字段转换类型时异常。{1}",
                    property.PropertyNo, e.Message);
                return null;
            }

            if (string.IsNullOrWhiteSpace(property.PropertyDecoration))
            {
                LogHelper.APILog(fileNameExtend, "房源[{0}]转委托失败，该房源的装饰类型为空", property.PropertyNo);
                return null;
            }
            var decorateType = GetDecorateType(property.PropertyDecoration);
            if (decorateType == null)
            {
                LogHelper.APILog(fileNameExtend, "房源[{0}]转委托失败，网站中没有对应的装饰类型[{1}]", property.PropertyNo, property.PropertyDecoration);
                return null;
            }
            entrustedInfo.DecorateTypeID = decorateType.ID;

            entrustedInfo.UsersID = user.ID;
            //加密客户手机
            //var encrypt = tripleDESCryptoHelper.Encrypt(user.Mobile);
            //entrustedInfo.Mobile = encrypt;
            entrustedInfo.Mobile = user.Mobile;
            entrustedInfo.Contents = property.Description ?? "";
            entrustedInfo.SourceType = 0; //SourceType (0:pc 1:wap)
            entrustedInfo.IsDel = 0;
            entrustedInfo.CreatDate = now;
            entrustedInfo.IsInputSys = 1;
            entrustedInfo.SysCode = property.PropertyID;
            entrustedInfo.InputDate = property.RegDate.HasValue ? property.RegDate.Value : now;
            entrustedInfo.SysAgentID = property.EmpID1;

            return entrustedInfo;
        }
        #endregion

        #region WEB数据库基础方法
        /// <summary>
        /// 根据类型获取价格范围
        /// </summary>
        /// <param name="price"></param>
        /// <param name="type">（1：小区 2二手房 3出租房）</param>
        /// <returns></returns>
        public B_PriceRange GetPriceRange(decimal price, int type)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = string.Format(@"SELECT * FROM B_PriceRange WHERE MinPrice <= {0} AND (MaxPrice > {0} OR MaxPrice = 0) AND EnumTypes = {1} AND IsDel = 0", price, type);
                return JJWeb.QueryFirstOrDefault<B_PriceRange>(sql);
            }
        }

        public B_AreaRange GetAreaRange(double square)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = string.Format(@"SELECT * FROM B_AreaRange WHERE MinArea <= {0} AND (MaxArea > {0} OR MaxArea = 0) AND IsDel = 0", square);
                return JJWeb.QueryFirstOrDefault<B_AreaRange>(sql);
            }
        }

        public B_DecorateType GetDecorateType(string name, bool ignoreDelete = true)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = "";
                if (ignoreDelete)
                {
                    sql = string.Format(@"SELECT * FROM B_DecorateType WHERE Name = '{0}'", name);
                }
                else
                {
                    sql = string.Format(@"SELECT * FROM B_DecorateType WHERE Name = '{0}' AND IsDel = 0", name);
                }
                return JJWeb.QueryFirstOrDefault<B_DecorateType>(sql);
            }
        }

        public U_Users GetUser(string mobile)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = string.Format(@"SELECT * FROM U_Users WHERE Mobile = '{0}' AND IsDel = 0", mobile);
                return JJWeb.QueryFirstOrDefault<U_Users>(sql);
            }
        }

        public O_Store GetStore(string sysCode, bool ignoreDelete = true)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = "";
                if (ignoreDelete)
                {
                    sql = string.Format(@"SELECT * FROM O_Store WHERE SysCode = '{0}' AND IsDel = 0", sysCode);
                }
                else
                {
                    sql = string.Format(@"SELECT * FROM O_Store WHERE SysCode = '{0}'", sysCode);
                }

                return JJWeb.QueryFirstOrDefault<O_Store>(sql);
            }
        }

        public O_Agent GetAgent(string sysCode, bool ignoreDelete = true)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = "";
                if (ignoreDelete)
                {
                    sql = string.Format(@"SELECT * FROM O_Agent WHERE SysCode = '{0}' AND IsDel = 0", sysCode);
                }
                else
                {
                    sql = string.Format(@"SELECT * FROM O_Agent WHERE SysCode = '{0}'", sysCode);
                }
                return JJWeb.QueryFirstOrDefault<O_Agent>(sql);
            }
        }

        public D_Building GetBuilding(string sysCode)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = string.Format(@"SELECT * FROM D_Building WHERE BuildingCode = (SELECT TOP 1 ZfkrBuildingCode FROM S_Building WHERE ID = '{0}')", sysCode);

                var buliding = JJWeb.QueryFirstOrDefault<D_Building>(sql);
                return buliding;
            }
        }

        public H_SaleHouse GetSaleHouse(string sysCode)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = string.Format(@"SELECT * FROM H_SaleHouse WHERE SysCode = '{0}'", sysCode);
                return JJWeb.QueryFirstOrDefault<H_SaleHouse>(sql);
            }
        }

        public H_RentalHouse GetRentalHouse(string sysCode)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = string.Format(@"SELECT * FROM H_RentalHouse WHERE SysCode = '{0}'", sysCode);
                return JJWeb.QueryFirstOrDefault<H_RentalHouse>(sql);
            }
        }

        public S_ContractInfo GetContractInfo(string contractNo)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = string.Format(@"SELECT * FROM S_ContractInfo WHERE ContractNo = '{0}'", contractNo);
                return JJWeb.QueryFirstOrDefault<S_ContractInfo>(sql);
            }
        }

        public S_ContractFollow GetContractFollow(string sysCode)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = string.Format(@"SELECT * FROM S_ContractFollow WHERE SysCode = '{0}'", sysCode);
                return JJWeb.QueryFirstOrDefault<S_ContractFollow>(sql);
            }
        }

        public H_SaleHouseImg GetSaleHouseImg(int saleHouseID, string imageUrl)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = string.Format(@"SELECT * FROM H_SaleHouseImg WHERE SaleHouseID='{0}' AND ImageUrl = '{1}'", saleHouseID, imageUrl);
                return JJWeb.QueryFirstOrDefault<H_SaleHouseImg>(sql);
            }
        }

        public H_RentalHouseImg GetRentalHouseImg(int rentalHouseID, string imageUrl)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = string.Format(@"SELECT * FROM H_RentalHouseImg WHERE RentalHouseID = '{0}' AND ImageUrl = '{1}'", rentalHouseID, imageUrl);
                return JJWeb.QueryFirstOrDefault<H_RentalHouseImg>(sql);
            }
        }

        public H_SaleHouseEvaluate GetSaleHouseEvaluate(string sysCode)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = string.Format(@"SELECT * FROM H_SaleHouseEvaluate WHERE SysCode = '{0}'", sysCode);
                return JJWeb.QueryFirstOrDefault<H_SaleHouseEvaluate>(sql);
            }
        }

        public H_RentalHouseEvaluate GetRentalHouseEvaluate(string sysCode)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = string.Format(@"SELECT * FROM H_RentalHouseEvaluate WHERE SysCode = '{0}'", sysCode);
                return JJWeb.QueryFirstOrDefault<H_RentalHouseEvaluate>(sql);
            }
        }

        public H_SaleHouseSeeLog GetSaleHouseSeeLog(string sysCode, int saleHouseID)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = string.Format(@"SELECT * FROM H_SaleHouseSeeLog WHERE SysCode = '{0}' AND SaleHouseID = {1}", sysCode, saleHouseID);
                return JJWeb.QueryFirstOrDefault<H_SaleHouseSeeLog>(sql);
            }
        }

        public H_RentalHouseSeeLog GetRentalHouseSeeLog(string sysCode, int rentalHouseID)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = string.Format(@"SELECT * FROM H_RentalHouseSeeLog WHERE SysCode = '{0}' AND RentalHouseID = {1}", sysCode, rentalHouseID);
                return JJWeb.QueryFirstOrDefault<H_RentalHouseSeeLog>(sql);
            }
        }

        public DateTime GetServerDate()
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = @"SELECT GETDATE() AS serverDate";
                return JJWeb.QueryFirstOrDefault<DateTime>(sql);
            }
        }

        public bool UpdateAgentHouseNum()
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                return JJWeb.Execute(@"UPDATE dbo.O_Agent SET EsfNum = (SELECT COUNT(1) FROM dbo.H_SaleHouse hsh WHERE HouseState = 0 AND hsh.AgentID = dbo.O_Agent.ID), CzfNum = (SELECT COUNT(1) FROM dbo.H_RentalHouse hrh WHERE HouseState = 0 AND hrh.AgentID = dbo.O_Agent.ID)") > 0;
            }
        }

        public bool UpdateStoreHouseNum()
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                return JJWeb.Execute(@"UPDATE dbo.O_Store SET EsfNum = (SELECT COUNT(1) FROM dbo.H_SaleHouse hsh JOIN dbo.O_Agent oa ON hsh.AgentID = oa.ID WHERE HouseState = 0 AND oa.StoreID = dbo.O_Store.ID), CzfNum = (SELECT COUNT(1) FROM dbo.H_RentalHouse hrh JOIN dbo.O_Agent oa ON hrh.AgentID = oa.ID WHERE HouseState = 0 AND oa.StoreID = dbo.O_Store.ID)") > 0;
            }
        }

        public bool UpdateBuildingHouseNum()
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                return JJWeb.Execute(@"UPDATE dbo.D_Building SET EsfNum = (SELECT COUNT(1) FROM dbo.H_SaleHouse hsh WHERE HouseState = 0 AND hsh.BuildingID = dbo.D_Building.BuildingCode), CzfNum = (SELECT COUNT(1) FROM dbo.H_RentalHouse hrh WHERE HouseState = 0 AND hrh.BuildingID = dbo.D_Building.BuildingCode)") > 0;
            }
        }

        public List<S_Building> GetMatchBuildingList(DateTime? beginDate)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = "";
                if (beginDate.HasValue)
                {
                    sql = string.Format(@"SELECT ID, BuildingName, BuildingAddress, ZfkrBuildingCode, ModDate
FROM S_Building WHERE ModDate >= '{0}' ORDER BY ModDate ASC", beginDate);
                }
                else
                {
                    sql = string.Format(@"SELECT ID, BuildingName, BuildingAddress, ZfkrBuildingCode, ModDate
FROM S_Building ORDER BY ModDate ASC");
                }
                return JJWeb.Query<S_Building>(sql).ToList();
            }
        }

        public S_Building GetMatchBuilding(string estateID)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = "";
                sql = string.Format(@"SELECT ID, BuildingName, BuildingAddress, ZfkrBuildingCode, ModDate
FROM S_Building WHERE ID = '{0}'", estateID);
                return JJWeb.QueryFirstOrDefault<S_Building>(sql);
            }
        }

        public bool UpdateEntrustedSysAgentID(string propertyID, string empID)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var sql = string.Format(@"UPDATE dbo.U_EntrustedInfo SET SysAgentID = '{0}' WHERE dbo.U_EntrustedInfo.SysCode = '{1}';", empID, propertyID);
                return JJWeb.Execute(sql) > 0;
            }
        }
        #endregion

        #region ERP数据库基础方法
        public List<Reference> GetDecorateTypeList(DateTime? beginDate)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = "";
                if (beginDate.HasValue)
                {
                    sql = string.Format(@"SELECT RefID, ItemValue, ItemNo, FlagDeleted, FlagTrashed, ModDate FROM Reference WHERE RefNameCn = '装修' AND ModDate >= '{0}' ORDER BY ModDate ASC", beginDate);
                }
                else
                {
                    sql = string.Format(@"SELECT RefID, ItemValue, ItemNo, FlagDeleted, FlagTrashed, ModDate FROM Reference WHERE RefNameCn = '装修' AND FlagTrashed = 0 AND FlagDeleted = 0 ORDER BY ModDate ASC");
                }
                return JJERP.Query<Reference>(sql).ToList();
            }
        }

        public List<Department> GetDepartmentList(DateTime? beginDate)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = "";
                if (beginDate.HasValue)
                {
                    sql = string.Format(@"SELECT DeptID, DeptNo, Layer, DeptName, FlagDeleted, FlagTrashed, Address, Tel, XLocate, YLocate, ModDate FROM Department WHERE FlagSale = 1 AND ModDate >= '{0}' ORDER BY ModDate ASC", beginDate);
                }
                else
                {
                    sql = string.Format(@"SELECT DeptID, DeptNo, Layer, DeptName, FlagDeleted, FlagTrashed, Address, Tel, XLocate, YLocate, ModDate FROM Department WHERE FlagSale = 1 AND FlagTrashed = 0 AND FlagDeleted = 0 ORDER BY ModDate ASC");
                }
                return JJERP.Query<Department>(sql).ToList();
            }
        }

        public List<Employee> GetEmployeeList(DateTime? beginDate)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = "";
                if (beginDate.HasValue)
                {
                    sql = string.Format(@"SELECT e.EmpID, e.EmpName, e.EmpNo, e.DeptID, e.mobile, e.StaffImage, e.WeixinNo, e.StaffWeiXinCodeUrl, e.JoinDate, e.ZFStatus, e.FlagTrashed, e.FlagDeleted, e.ModDate, p.PositionName FROM Employee e LEFT JOIN Position p ON e.PositionID = p.PositionID WHERE e.ModDate >= '{0}' ORDER BY e.ModDate ASC", beginDate);
                }
                else
                {
                    sql = string.Format(@"SELECT e.EmpID, e.EmpName, e.EmpNo, e.DeptID, e.mobile, e.StaffImage, e.WeixinNo, e.StaffWeiXinCodeUrl, e.JoinDate, e.ZFStatus, e.FlagTrashed, e.FlagDeleted, e.ModDate, p.PositionName FROM Employee e LEFT JOIN Position p ON e.PositionID = p.PositionID WHERE p.PositionName LIKE '%经纪人' AND e.FlagDeleted = 0 AND e.FlagTrashed = 0 AND e.ZFStatus <> '离职' ORDER BY e.ModDate ASC");
                }

                return JJERP.Query<Employee>(sql).ToList();
            }
        }

        public List<BeltWatch> GetBeltWatchList(DateTime? beginDate, int topN)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = "";
                if (beginDate.HasValue)
                {
                    sql = string.Format(@"SELECT TOP {0} BeltWatchID, BeltWatchGroupID, StatusInt, BeltWatchDate, EmpID, ModDate, PropertyNo, InquiryNo FROM BeltWatch WHERE ModDate >= '{1}' ORDER BY ModDate ASC", topN, beginDate);
                }
                else
                {
                    sql = string.Format(@"SELECT TOP {0} BeltWatchID, BeltWatchGroupID, StatusInt, BeltWatchDate, EmpID, ModDate, PropertyNo, InquiryNo FROM BeltWatch WHERE StatusInt = 1 OR StatusInt IS NULL ORDER BY ModDate ASC", topN);
                }
                return JJERP.Query<BeltWatch>(sql).ToList();
            }
        }

        public List<Contract> GetContractInfoList(DateTime? beginDate)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = "";
                if (beginDate.HasValue)
                {
                    sql = string.Format(@"SELECT * FROM Contract WHERE ModDate >= '{0}' ORDER BY ModDate ASC", beginDate);
                }
                else
                {
                    sql = string.Format(@"SELECT * FROM Contract WHERE FlagTrashed = 0 AND FlagDeleted = 0 ORDER BY ModDate ASC");
                }
                return JJERP.Query<Contract>(sql).ToList();
            }
        }

        public List<Flow> GetContractFollowList(DateTime? beginDate)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = "";
                if (beginDate.HasValue)
                {
                    //只获取在职经纪人
                    sql = string.Format(@"SELECT * FROM Flow WHERE ModDate >= '{0}' ORDER BY ModDate ASC", beginDate);
                }
                else
                {
                    sql = string.Format(@"SELECT * FROM Flow WHERE FlagTrashed = 0 AND FlagDeleted = 0 ORDER BY ModDate ASC");
                }
                return JJERP.Query<Flow>(sql).ToList();
            }
        }

        public List<ImageInfo> GetPropertyImageList(DateTime? beginDate)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = "";
                if (beginDate.HasValue)
                {
                    //只获取在职经纪人
                    sql = string.Format(@"SELECT ImageID, EmpID, ImageUrl, PropertyID, ModDate, FlagTrashed, FlagDelete FROM ImageInfo WHERE (ImageType = '房勘' OR ImageType = '原有图片') AND ModDate >= '{0}' ORDER BY ModDate ASC", beginDate);
                }
                else
                {
                    sql = string.Format(@"SELECT ImageID, EmpID, ImageUrl, PropertyID, ModDate, FlagTrashed, FlagDelete FROM ImageInfo WHERE FlagTrashed = 0 AND FlagDelete = 0 AND (ImageType = '房勘' OR ImageType = '原有图片') ORDER BY ModDate ASC");
                }
                return JJERP.Query<ImageInfo>(sql).ToList();
            }
        }

        public List<PropertyComment> GetPropertyEvaluateList(DateTime? beginDate)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = "";
                if (beginDate.HasValue)
                {
                    //只获取在职经纪人
                    sql = string.Format(@"SELECT * FROM PropertyComment WHERE CommentDate >= '{0}' ORDER BY CommentDate ASC", beginDate);
                }
                else
                {
                    sql = string.Format(@"SELECT * FROM PropertyComment WHERE FlagTrashed = 0 AND FlagDeleted = 0 ORDER BY CommentDate ASC");
                }
                return JJERP.Query<PropertyComment>(sql).ToList();
            }
        }

        public List<Property> GetPropertyList(DateTime? beginDate)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = "";
                if (beginDate.HasValue)
                {
                    sql = string.Format(@"SELECT PropertyID, PropertyNo, Title, PropertyDirection, TagDescription, Square, CountT, CountF, CountW, FloorAll, Floor, UnitName, Price, EstateID, PropertyDecoration, EmpID1, PropertyCommission, RentUnitName, RentPrice, Trade, CheckTimes, Status, IsOrder, FlagDeleted, FlagTrashed, ModDate, EntrustPhotoCount, ExclusivePhotoCount, RentType, PropertyUsage FROM Property WHERE ModDate >= '{0}' ORDER BY ModDate ASC", beginDate);
                }
                else
                {
                    sql = string.Format(@"SELECT PropertyID, PropertyNo, Title, PropertyDirection, TagDescription, Square, CountT, CountF, CountW, FloorAll, Floor, UnitName, Price, EstateID, PropertyDecoration, EmpID1, PropertyCommission, RentUnitName, RentPrice, Trade, CheckTimes, Status, IsOrder, FlagDeleted, FlagTrashed, ModDate, EntrustPhotoCount, ExclusivePhotoCount, RentType, PropertyUsage FROM Property WHERE RunningStatus IN(1, 2) AND FlagTrashed = 0 AND FlagDeleted = 0 AND (EntrustPhotoCount > 0 OR ExclusivePhotoCount > 0) ORDER BY ModDate ASC");
                }
                return JJERP.Query<Property>(sql).ToList();
            }
        }

        public List<Property> GetPropertyList(string estateID)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = "";
                sql = string.Format(@"SELECT PropertyID, PropertyNo, Title, PropertyDirection, TagDescription, Square, CountT, CountF, CountW, FloorAll, Floor, UnitName, Price, EstateID, PropertyDecoration, EmpID1, PropertyCommission, RentUnitName, RentPrice, Trade, CheckTimes, Status, IsOrder, FlagDeleted, FlagTrashed, ModDate, EntrustPhotoCount, ExclusivePhotoCount, RentType, PropertyUsage FROM Property WHERE RunningStatus IN(1, 2) AND FlagTrashed = 0 AND FlagDeleted = 0 AND (EntrustPhotoCount > 0 OR ExclusivePhotoCount > 0) AND EstateID = '{0}'", estateID);
                return JJERP.Query<Property>(sql).ToList();
            }
        }

        public List<Property> GetValidPropertyList(string mobile)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                //var sql = string.Format(@"SELECT * FROM Property WHERE RunningStatus IN(1, 2) AND (OwnerMobile = '{0}' OR OwnerTel = '{0}' OR ContactTel3 = '{0}');", mobile);
                var sql = string.Format(@"SELECT * FROM Property WHERE RunningStatus IN(1, 2) AND OwnerMobile = '{0}';", mobile);
                var list = JJERP.Query<Property>(sql).ToList();
                return list;
            }
        }

        public List<Inquiry> GetValidInquiryList(string mobile)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                //var sql = string.Format(@"SELECT * FROM Inquiry WHERE (RunningStatus = 1 OR (RunningStatus = 2 AND TransferTimes < 4)) AND (CustMobile = '{0}' OR CustTel = '{0}')", mobile);
                var sql = string.Format(@"SELECT * FROM Inquiry WHERE (RunningStatus = 1 OR (RunningStatus = 2 AND TransferTimes < 4)) AND CustMobile = '{0}'", mobile);
                var list = JJERP.Query<Inquiry>(sql).ToList();
                return list;
            }
        }

        public List<Contract> GetPropertyContractInfoList(string propertyNoStr)
        {
            if (propertyNoStr == "''")
            {
                return new List<Contract>();
            }
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = string.Format(@"SELECT * FROM Contract WHERE PropertyNo IN ({0}) AND FlagTrashed = 0 AND FlagDeleted = 0", propertyNoStr);
                var list = JJERP.Query<Contract>(sql).ToList();
                return list;
            }
        }

        public List<Contract> GetInquiryContractInfoList(string inquiryNoStr)
        {
            if (inquiryNoStr == "''")
            {
                return new List<Contract>();
            }
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = string.Format(@"SELECT * FROM Contract WHERE InquiryNo IN ({0}) AND FlagTrashed = 0 AND FlagDeleted = 0", inquiryNoStr);
                var list = JJERP.Query<Contract>(sql).ToList();
                return list;
            }
        }

        public List<ContractReport> GetContractReportList(string contractNoStr)
        {
            if (contractNoStr == "''")
            {
                return new List<ContractReport>();
            }
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = string.Format(@"SELECT * FROM ContractReport WHERE ContractNo IN ({0})", contractNoStr);
                return JJERP.Query<ContractReport>(sql).ToList();
            }
        }

        public List<ImageInfo> GetPropertyImageList(string propertyIDStr)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = string.Format(@"SELECT ImageID, EmpID, ImageUrl, PropertyID, ModDate, FlagTrashed, FlagDelete FROM ImageInfo WHERE PropertyID IN ({0}) AND (ImageType = '房勘' OR ImageType = '原有图片') AND FlagTrashed = 0 AND FlagDelete = 0 ORDER BY ImageID DESC", propertyIDStr);
                return JJERP.Query<ImageInfo>(sql).ToList();
            }
        }

        public List<PropertyComment> GetPropertyEvaluateList(string propertyIDStr)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = "";
                sql = string.Format(@"SELECT * FROM PropertyComment WHERE FlagTrashed = 0 AND FlagDeleted = 0 AND PropertyID IN ({0}) ORDER BY CommentDate ASC", propertyIDStr);
                return JJERP.Query<PropertyComment>(sql).ToList();
            }
        }

        public List<BeltWatch> GetBeltWatchList(string propertyNoStr)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = "";
                sql = string.Format(@"SELECT BeltWatchID, BeltWatchGroupID, StatusInt, BeltWatchDate, EmpID, ModDate, PropertyNo, InquiryNo FROM BeltWatch WHERE PropertyNo IN ({0})", propertyNoStr);
                return JJERP.Query<BeltWatch>(sql).ToList();
            }
        }

        public List<Flow> GetContractFollowList(string contractIDStr)
        {
            if (contractIDStr == "''")
            {
                return new List<Flow>();
            }
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = string.Format(@"SELECT * FROM Flow WHERE ContractID IN ({0}) AND FlagTrashed = 0 AND FlagDeleted = 0", contractIDStr);
                var list = JJERP.Query<Flow>(sql).ToList();
                return list;
            }
        }

        public Estate GetEstate(string estateName)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = string.Format(@"SELECT * FROM Estate WHERE EstateName = '{0}' AND FlagTrashed = 0 AND FlagDeleted = 0", estateName);
                return JJERP.QueryFirstOrDefault<Estate>(sql);
            }
        }

        public Property GetPropertyByNo(string propertyNo)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = string.Format(@"SELECT * FROM Property WHERE PropertyNo = '{0}'", propertyNo);
                return JJERP.QueryFirstOrDefault<Property>(sql);
            }
        }

        public Property GetPropertyByID(string propertyID)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = string.Format(@"SELECT * FROM Property WHERE PropertyID = '{0}'", propertyID);
                return JJERP.QueryFirstOrDefault<Property>(sql);
            }
        }

        public Inquiry GetInquiryByNo(string inquiryNo)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = string.Format(@"SELECT * FROM Inquiry WHERE InquiryNo = '{0}'", inquiryNo);
                return JJERP.QueryFirstOrDefault<Inquiry>(sql);
            }
        }

        public List<ImageInfo> GetPropertyImageUrlList(string propertyID)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = string.Format(@"SELECT ImageUrl FROM ImageInfo WHERE PropertyID = '{0}' AND (ImageType = '房勘' OR ImageType = '原有图片') AND FlagTrashed = 0 AND FlagDelete = 0 ORDER BY ImageID DESC", propertyID);
                return JJERP.Query<ImageInfo>(sql).ToList();
            }
        }

        public List<ImageInfo> GetDepartmentImageList(string deptID)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = string.Format(@"SELECT ImageUrl FROM ImageInfo WHERE CommonID = '{0}' AND ImageType = '部门' AND FlagTrashed = 0 AND FlagDelete = 0 ORDER BY ModDate DESC", deptID);
                return JJERP.Query<ImageInfo>(sql).ToList();
            }
        }

        public ContractReport GetContractReportByNo(string contractNo)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = string.Format(@"SELECT * FROM ContractReport WHERE ContractNo = '{0}'", contractNo);
                return JJERP.QueryFirstOrDefault<ContractReport>(sql);
            }
        }

        public ContractReport GetContractReportByID(string reportID)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = string.Format(@"SELECT * FROM ContractReport WHERE ContractReportID = '{0}'", reportID);
                return JJERP.QueryFirstOrDefault<ContractReport>(sql);
            }
        }
        #endregion

        public bool IsBeltWatchGroupHouseAllInWebDB(string groupID, string tradeType)
        {
            if (string.IsNullOrWhiteSpace(groupID))
            {
                return false;
            }

            List<string> propertyIDList = null;
            int houseInWebDBcount = 0;
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var sql = string.Format(@"SELECT p.PropertyID FROM dbo.BeltWatch bw JOIN dbo.Property p ON p.PropertyNo = bw.PropertyNo WHERE bw.BeltWatchGroupID = '{0}';", groupID);
                propertyIDList = JJERP.Query<string>(sql).ToList();
            }

            if (propertyIDList == null || propertyIDList.Count == 0)
            {
                return false;
            }

            var IDStr = string.Join("','", propertyIDList);
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                string sql;
                if (tradeType == "求购")
                {
                    sql = string.Format(@"SELECT COUNT(1) FROM dbo.H_SaleHouse hsh WHERE hsh.SysCode IN('{0}')", IDStr);
                }
                else if (tradeType == "求租")
                {
                    sql = string.Format(@"SELECT COUNT(1) FROM dbo.H_RentalHouse hrh WHERE hrh.SysCode IN('{0}')", IDStr);
                }
                else
                {
                    return false;
                }
                houseInWebDBcount = JJWeb.ExecuteScalar<int>(sql);
            }

            if (houseInWebDBcount != propertyIDList.Count())
            {
                return false;
            }
            return true;
        }

        #region 公共方法
        public Response VerifyNotNull<T>(T entity)
        {
            var type = entity.GetType();
            var isValid = true;
            var nullList = new List<string>();
            foreach (var item in type.GetProperties())
            {
                object[] objAttrs = item.GetCustomAttributes(typeof(NotNullAttribute), false);
                if (objAttrs.Length > 0)
                {
                    if (item.GetValue(entity) == null)
                    {
                        isValid = false;
                        nullList.Add((objAttrs[0] as NotNullAttribute).ColumnNameCN);
                    }
                }
            }

            return new Response()
            {
                StatusCode = isValid ? 1 : 0,
                StatusMsg = isValid ? "验证通过" : "验证不通过",
                Data = isValid ? null : nullList
            };
        }

        public bool WebExecute(string sql)
        {
            using (SqlConnection JJWeb = OpenConnection(2))
            {
                var count = 0;
                count = JJWeb.Execute(sql);
                return count > 0;
            }
        }

        public bool ERPExecute(string sql)
        {
            using (SqlConnection JJERP = OpenConnection(1))
            {
                var count = 0;
                count = JJERP.Execute(sql);
                return count > 0;
            }
        }

        public string BuildSql<T>(SqlBuildTypeEnum buildType, T entity) where T : new()
        {
            var sql = "";
            if (buildType == SqlBuildTypeEnum.Insert)
            {
                sql = BuildInsertSql(entity);
            }
            else if (buildType == SqlBuildTypeEnum.Update || buildType == SqlBuildTypeEnum.LogicDelete)
            {
                sql = BuildUpdateSql(entity);
            }
            else if (buildType == SqlBuildTypeEnum.RealDelete)
            {
                sql = BuildDeleteSql(entity);
            }

            return sql;
        }

        public string BuildInsertSql<T>(T entity) where T : new()
        {
            var type = entity.GetType();
            var sql = "";
            var tableName = type.Name;
            var columnName = "";
            var values = "";

            foreach (var item in type.GetProperties())
            {
                var objAttrs = item.GetCustomAttributes(typeof(AutoIncrementPrimaryKeyAttribute), false);
                var value = item.GetValue(entity);

                if (objAttrs.Length <= 0)
                {
                    columnName += string.Format("{0},", item.Name);
                    values += string.Format("'{0}',", value);
                }
            }

            columnName = columnName.Substring(0, columnName.Length - 1);
            values = values.Substring(0, values.Length - 1);

            sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", tableName, columnName, values);
            return sql;
        }

        public string BuildUpdateSql<T>(T entity) where T : new()
        {
            var type = entity.GetType();
            var sql = "";
            var tableName = type.Name;
            var set = "";
            var where = "";

            foreach (var item in type.GetProperties())
            {
                var objAttrs = item.GetCustomAttributes(typeof(AutoIncrementPrimaryKeyAttribute), false);
                var value = item.GetValue(entity);

                if (objAttrs.Length <= 0)
                {
                    set += string.Format("{0}='{1}',", item.Name, value);
                }
                else
                {
                    where = string.Format("{0}='{1}'", item.Name, value);
                }
            }

            set = set.Substring(0, set.Length - 1);

            sql = string.Format("UPDATE {0} SET {1} WHERE {2}", tableName, set, where);
            return sql;
        }

        public string BuildDeleteSql<T>(T entity) where T : new()
        {
            var type = entity.GetType();
            var sql = "";
            var tableName = type.Name;
            var where = "";

            foreach (var item in type.GetProperties())
            {
                var objAttrs = item.GetCustomAttributes(typeof(AutoIncrementPrimaryKeyAttribute), false);
                var value = item.GetValue(entity);

                if (objAttrs.Length > 0)
                {
                    where = string.Format("{0}='{1}'", item.Name, value);
                }
            }

            sql = string.Format("DELETE FROM {0} WHERE {1}", tableName, where);
            return sql;
        }

        public class DBEntity<T> where T : new()
        {
            public SqlBuildTypeEnum BuildType { get; set; }
            public T Entity { get; set; }

            public DBEntity(SqlBuildTypeEnum buildType, T entity)
            {
                this.BuildType = buildType;
                this.Entity = entity;
            }
        }
        #endregion
    }
}