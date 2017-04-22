using RCApp_Win.DataModule.CustomerManager.BaseManage;
using RCApp_Win.DataModule.Employment.DepartmentMgr;
using RCApp_Win.DataModule.Employment.EmployeeMgr;
using RCApp_Win.DataModule.SalaryManager;
using RCApp_Win.DataModule.SalaryManager.ContractManage;
using RCApp_Win.Logic.Image;
using RCApp_Win.Model.DbModel;
using RCApp_Win.Model.Image;
using RCApp_Win.MyMessageBox;
using RCApp_Win.Util;
using RCApp_Win.View.Common;
using RCApp_Win.View.Resource;
using RCApp_Win;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RCApp_Win.Logic.Utility;
using RCApp_Win.Logic.House;
using RCApp_Win.Model.ViewModel.EmployeementVM;

namespace RCApp_Win.View.Salary
{
    public partial class A1AwardPunishAddWindow
    {
        private AwardPunishReportController awardPunishReportController = new AwardPunishReportController();
        private ContractController contractController = new ContractController();
        private EmployeeController employeeController = new EmployeeController();
        private SalaryController salaryController = new SalaryController();
        private EmployeeX CurrentEmp = null;
        private int BaseScore = -1;

        #region 输入框与类型对应关系
        //PunishResult: 1代表扣分,2代表扣款
        //Type: 1代表奖励，2代表考勤惩罚，3代表督导惩罚
        //Times次数，ActualMoney实扣，Points扣分，PointsTimes扣分倍数
        private List<LoopUpdate> LoopUpdateList = new List<LoopUpdate>() { 
            new LoopUpdate() 
            {
                levelID = 4,levelName="全勤",PunishResult="",Type=1,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_FullAttendanceMultiple",propName="PointsTimes",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 3,levelName="全佣",PunishResult="",Type=1,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_FullCommissionMultiple",propName="PointsTimes",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 5,levelName="带看规范性",PunishResult="",Type=1,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_GuideWatchMultiple",propName="PointsTimes",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 6,levelName="租赁奖励",PunishResult="",Type=1,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_RentHouseAwardScore",propName="Points",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 7,levelName="举报奖励",PunishResult="",Type=1,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_ReportAwardScore",propName="Points",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 9,levelName="迟到",PunishResult="1",Type=2,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_LateCount",propName="Times",valueType=1},
                    new PropBind(){txtBoxName="txt_LateMultiple",propName="PointsTimes",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 10,levelName="早退",PunishResult="1",Type=2,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_LeaveEarlyCount",propName="Times",valueType=1},
                    new PropBind(){txtBoxName="txt_LeaveEarlyMultiple",propName="PointsTimes",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 11,levelName="代打卡",PunishResult="1",Type=2,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_HelpCheckInCount",propName="Times",valueType=1},
                    new PropBind(){txtBoxName="txt_HelpCheckInMultiple",propName="PointsTimes",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 12,levelName="未打卡",PunishResult="1",Type=2,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_NotCheckInCount",propName="Times",valueType=1},
                    new PropBind(){txtBoxName="txt_NotCheckInMultiple",propName="PointsTimes",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 13,levelName="旷工",PunishResult="1",Type=2,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_AbsenteeismCount",propName="Times",valueType=1},
                    new PropBind(){txtBoxName="txt_AbsenteeismMultiple",propName="PointsTimes",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 14,levelName="事假",PunishResult="1",Type=2,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_AffairLeaveCount",propName="Times",valueType=1},
                    new PropBind(){txtBoxName="txt_AffairLeaveMultiple",propName="PointsTimes",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 15,levelName="病假",PunishResult="1",Type=2,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_SickLeaveCount",propName="Times",valueType=1},
                    new PropBind(){txtBoxName="txt_SickLeaveMultiple",propName="PointsTimes",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 21,levelName="工作量",PunishResult="1",Type=3,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_WorkLoadLack",propName="Times",valueType=1},
                    new PropBind(){txtBoxName="txt_WorkLoadLackMultiple",propName="PointsTimes",valueType=2},
                    new PropBind(){txtBoxName="txt_WorkLoadLackDeductScore",propName="Points",valueType=2},
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 24,levelName="虚假房源",PunishResult="1",Type=3,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_PropertyValidMultiple",propName="PointsTimes",valueType=2},
                    new PropBind(){txtBoxName="txt_PropertyValidDeductScore",propName="Points",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 26,levelName="虚假带看",PunishResult="1",Type=3,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_GuideWatchValidMultiple",propName="PointsTimes",valueType=2},
                    new PropBind(){txtBoxName="txt_GuideWatchValidDeductScore",propName="Points",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 27,levelName="客户录入",PunishResult="1",Type=3,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_InquiryVaildMultiple",propName="PointsTimes",valueType=2},
                    new PropBind(){txtBoxName="txt_InquiryVaildDeductScore",propName="Points",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 28,levelName="举报处罚",PunishResult="1",Type=3,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_BeReportedMultiple",propName="PointsTimes",valueType=2},
                    new PropBind(){txtBoxName="txt_BeReportedDeductScore",propName="Points",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 17,levelName="着装",PunishResult="1",Type=3,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_DressUnsuitableMultiple",propName="PointsTimes",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 19,levelName="外出未签",PunishResult="1",Type=3,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_OutWithoutRegisterMultiple",propName="PointsTimes",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 30,levelName="黄线标准",PunishResult="1",Type=3,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_YellowLineMultiple",propName="PointsTimes",valueType=2}
                }
            } ,
            new LoopUpdate() 
            {
                levelID = 31,levelName="客户投诉",PunishResult="1",Type=3,
                propBingList=new List<PropBind>(){
                    new PropBind(){txtBoxName="txt_CustomerComplaintsMultiple",propName="PointsTimes",valueType=2}
                }
            }
        };
        #endregion

        public A1AwardPunishAddWindow()
        {
            InitializeComponent();

            this.CancelDelegateEvent += Button_Click_Cancel;
            this.ConfirmDelegateEvent += Button_Click_Confirm;
        }

        private void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            loadstyle();

            autotxt_EmpName.OnTextChange += autotxtEmpNameChanged;
            autotxt_EmpName.OnKeyDownEvent += autotxtEmpNameKeyDown;
            autotxt_EmpName.OnSelectionChanged += autotxtEmpNameSelectChanged;
            autotxt_EmpName.textBox.Foreground = Brushes.Black;
            autotxt_EmpName.searchThreshold = 1;
        }

        private void Button_Click_Confirm(object sender, RoutedEventArgs e)
        {
            if (CurrentEmp == null)
            {
                YZWmessagebox.Show("请先选择待奖惩的人员");
                return;
            }
            if (BaseScore < 0)
            {
                YZWmessagebox.Show("员工基础分获取失败");
                return;
            }
            LoopSaveData();
        }

        public void LoopSaveData()
        {
            var APStandard = contractController.GetAllAPStandard();
            var today = CommonController.getServerDate();

            var resultList = new List<EmpAwardAndPunish>();
            var type = typeof(EmpAwardAndPunish);
            var hasInput = false;

            foreach (var item in LoopUpdateList)
            {
                var empAwardAndPunish = new EmpAwardAndPunish();
                hasInput = false;

                foreach (var propBind in item.propBingList)
                {
                    var tb = VisualHelperTreeExtension.GetChildByName<TextBox>(grd_Main, propBind.txtBoxName);
                    if (!string.IsNullOrWhiteSpace(tb.Text))
                    {
                        var prop = type.GetProperty(propBind.propName);
                        switch (propBind.valueType)
                        {
                            case 1:
                                var intValue = GetValueSafely<int>(tb.Text);
                                if (!intValue.Item1 || !SetPropSafely(empAwardAndPunish, intValue.Item2, prop))
                                {
                                    NotifyTextBox(tb);
                                    return;
                                }
                                break;
                            case 2:
                                var decimalValue = GetValueSafely<decimal>(tb.Text);
                                if (!decimalValue.Item1 || !SetPropSafely(empAwardAndPunish, decimalValue.Item2, prop))
                                {
                                    NotifyTextBox(tb);
                                    return;
                                }
                                break;
                            default: break;
                        }
                        hasInput = true;
                    }
                }

                if (!hasInput)
                {
                    continue;
                }

                if (!empAwardAndPunish.ActualMoney.HasValue)
                {
                    var tbProp = item.propBingList.FirstOrDefault(i => i.propName == "ActualMoney");
                    if (tbProp != null)
                    {
                        var tb = VisualHelperTreeExtension.GetChildByName<TextBox>(grd_Main, tbProp.txtBoxName);
                        NotifyTextBox(tb);
                        return;
                    }
                }

                if (!empAwardAndPunish.Points.HasValue)
                {
                    if (item.propBingList.Select(i => i.propName).Contains("PointsTimes"))
                    {
                        if (empAwardAndPunish.PointsTimes.HasValue)
                        {
                            empAwardAndPunish.Points = empAwardAndPunish.PointsTimes * BaseScore;
                        }
                        else
                        {
                            var tbProp = item.propBingList.FirstOrDefault(i => i.propName == "PointsTimes");
                            var tb = VisualHelperTreeExtension.GetChildByName<TextBox>(grd_Main, tbProp.txtBoxName);
                            NotifyTextBox(tb);
                            return;
                        }
                    }
                    else
                    {
                        var tbProp = item.propBingList.FirstOrDefault(i => i.propName == "Points");
                        var tb = VisualHelperTreeExtension.GetChildByName<TextBox>(grd_Main, tbProp.txtBoxName);
                        NotifyTextBox(tb);
                        return;
                    }
                }

                if (empAwardAndPunish.ActualMoney == 0 || empAwardAndPunish.Points == 0)
                {
                    continue;
                }
                else
                {
                    empAwardAndPunish.ActualMoney = empAwardAndPunish.ActualMoney.HasValue ? empAwardAndPunish.ActualMoney : 0;
                    empAwardAndPunish.Points = empAwardAndPunish.Points.HasValue ? empAwardAndPunish.Points : 0;
                    empAwardAndPunish.PointsTimes = empAwardAndPunish.PointsTimes.HasValue ? empAwardAndPunish.PointsTimes : 0;
                    empAwardAndPunish.Times = empAwardAndPunish.Times.HasValue ? empAwardAndPunish.Times : 0;
                }

                var standard = APStandard.FirstOrDefault(i => i.LevelName == item.levelName);
                if (standard == null)
                {
                    YZWmessagebox.Show(string.Format("奖惩类型【{0}】无法识别，请联系管理员", item.levelName));
                    return;
                }

                empAwardAndPunish.EmpDJ = CurrentEmp.LevelStr;
                empAwardAndPunish.EmpBasePoint = BaseScore;
                empAwardAndPunish.EmpID = CurrentEmp.EmpID;
                empAwardAndPunish.IsPunished = 1;
                empAwardAndPunish.IsProof = 0;
                empAwardAndPunish.PunishResult = item.PunishResult;
                empAwardAndPunish.LevelID = standard.StandardID;
                empAwardAndPunish.RecordEmpID = App.session.loginEmpID;
                empAwardAndPunish.Type = item.Type;
                empAwardAndPunish.FlagDeleted = false;
                empAwardAndPunish.FlagTrashed = false;
                empAwardAndPunish.Memo = "";
                empAwardAndPunish.VerifyStatus = 0;
                empAwardAndPunish.ExpectedMoney = 0;

                empAwardAndPunish.Datetime = today;
                empAwardAndPunish.UpdateDateTime = today;
                if (item.Type != 1)
                {
                    empAwardAndPunish.ViolationDateTime = today;
                    empAwardAndPunish.CheckDateTime = today;
                }

                resultList.Add(empAwardAndPunish);
            }
            if (resultList.Count > 0)
            {
                var saveCount = awardPunishReportController.AwardAndPunishBatchSave(resultList);
                if (saveCount > 0)
                {
                    YZWmessagebox.Show("保存成功");
                    this.Close();
                }
                else
                {
                    YZWmessagebox.Show("保存失败");
                }
            }
        }

        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void loadstyle()
        {
            List<ResourceDictionary> listresourse = new List<ResourceDictionary>();
            if (App.appThemes == 0)
            {
                this.Resources.MergedDictionaries[0] = new ResourceDictionary()
                {
                    Source = new Uri("../../Themes/Styles/Common/ScrollViewerStyle.xaml", UriKind.Relative)
                };
                this.Resources.MergedDictionaries[1] = new ResourceDictionary()
                {
                    Source = new Uri("../../Themes/Styles/Common/TabItemStyle.xaml", UriKind.Relative)
                };
                this.Resources.MergedDictionaries[2] = new ResourceDictionary()
                {
                    Source = new Uri("../../Themes/Styles/Common/ComboBox.xaml", UriKind.Relative)
                };
                //this.Resources.MergedDictionaries[3] = new ResourceDictionary()
                //{
                //    Source = new Uri("../../Themes/Styles/Common/CommonStyles.xaml", UriKind.Relative)
                //};
            }
            else if (App.appThemes == 1)
            {
                this.Resources.MergedDictionaries[0] = new ResourceDictionary()
                {
                    Source = new Uri("../../Themes/Styles/Common/ScrollViewerStyle.xaml", UriKind.Relative)
                };
                this.Resources.MergedDictionaries[1] = new ResourceDictionary()
                {
                    Source = new Uri("../../Themes/Styles/Common/TabItemStyleBlue.xaml", UriKind.Relative)
                };
                this.Resources.MergedDictionaries[2] = new ResourceDictionary()
                {
                    Source = new Uri("../../Themes/Styles/Common/ComboBoxBlue.xaml", UriKind.Relative)
                };
                //this.Resources.MergedDictionaries[3] = new ResourceDictionary()
                //{
                //    Source = new Uri("../../Themes/Styles/Common/CommonStylesBlue.xaml", UriKind.Relative)
                //};
            }
        }

        #region 事件_自动补全控件
        private void autotxtEmpNameChanged()
        {
            if (autotxt_EmpName.comboBox.ItemsSource != null)
            {
                autotxt_EmpName.comboBox.ItemsSource = null;
            }

            if (autotxt_EmpName.textBox.Text.Length >= autotxt_EmpName.searchThreshold)
            {
                List<EmployeeX> empList = employeeController.EmployeeFuzzyQuery(autotxt_EmpName.textBox.Text);

                autotxt_EmpName.comboBox.ItemsSource = empList;
                autotxt_EmpName.comboBox.DisplayMemberPath = "EmpName";

                autotxt_EmpName.comboBox.IsDropDownOpen = autotxt_EmpName.comboBox.HasItems;
            }
            else
            {
                autotxt_EmpName.comboBox.IsDropDownOpen = false;
            }
        }

        private void autotxtEmpNameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (autotxt_EmpName.comboBox.IsDropDownOpen)
                {
                    autotxt_EmpName.comboBox.SelectedIndex = 0;
                }
            }
        }

        private void autotxtEmpNameSelectChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var item = autotxt_EmpName.comboBox.SelectedItem as EmployeeX;
            if (item != null)
            {
                try
                {
                    CurrentEmp = item;
                    autotxt_EmpName.Text = item.EmpName;
                    txt_deptName.Text = item.DeptName;
                    txt_deptManager.Text = item.DeptManage;

                    LevelBase lev = new LevelController().GetLevelInfoByName(item.LevelStr);
                    if (lev.BasePoints.HasValue)
                    {
                        BaseScore = lev.BasePoints.Value;
                        txt_empBaseAndScore.Text = item.LevelStr + "/" + BaseScore;
                    }
                    else
                    {
                        NotifyTextBox(txt_empBaseAndScore);
                        BaseScore = -1;
                        txt_empBaseAndScore.Text = item.LevelStr + "/ ?";
                        NotifyTextBox(txt_empBaseAndScore);
                    }
                }
                catch
                {
                    NotifyTextBox(txt_empBaseAndScore);
                    YZWmessagebox.Show("员工基础分获取失败");
                    BaseScore = -1;
                }
            }
        }
        #endregion

        private void GlobalTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            switch (textBox.Name)
            {
                //①奖励积分
                case "txt_FullAttendanceMultiple":
                case "txt_FullCommissionMultiple":
                case "txt_GuideWatchMultiple":
                    SumTextBoxDecimal(txt_AwardMultipleSum, txt_FullAttendanceMultiple, txt_FullCommissionMultiple, txt_GuideWatchMultiple);
                    Multiplication(txt_AwardScorePart1Sum, txt_AwardMultipleSum);
                    break;
                case "txt_AwardScorePart1Sum":
                case "txt_RentHouseAwardScore":
                case "txt_ReportAwardScore":
                    SumTextBoxDecimal(txt_AwardScoreSum, txt_AwardScorePart1Sum, txt_RentHouseAwardScore, txt_ReportAwardScore);
                    break;
                //②考勤惩罚积分
                case "txt_LateCount":
                case "txt_LeaveEarlyCount":
                case "txt_HelpCheckInCount":
                case "txt_NotCheckInCount":
                case "txt_AbsenteeismCount":
                case "txt_AffairLeaveCount":
                case "txt_SickLeaveCount":
                    break;
                case "txt_LateMultiple":
                case "txt_LeaveEarlyMultiple":
                case "txt_HelpCheckInMultiple":
                case "txt_NotCheckInMultiple":
                case "txt_AbsenteeismMultiple":
                case "txt_AffairLeaveMultiple":
                case "txt_SickLeaveMultiple":
                    SumTextBoxDecimal(txt_AttendanceMultipleSum, txt_LateMultiple, txt_LeaveEarlyMultiple, txt_HelpCheckInMultiple, txt_NotCheckInMultiple, txt_AbsenteeismMultiple, txt_AffairLeaveMultiple, txt_SickLeaveMultiple);
                    Multiplication(txt_AttendanceDeductScoreSum, txt_AttendanceMultipleSum);
                    break;
                //③工作量惩罚积分
                case "txt_WorkLoadLack":
                    break;
                case "txt_WorkLoadLackMultiple":
                    Multiplication(txt_WorkLoadLackDeductScore, txt_WorkLoadLackMultiple);
                    break;
                //④信息有效性惩罚积分
                case "txt_PropertyValidMultiple":
                    SumTextBoxDecimal(txt_InfoValidMultipleSum, txt_PropertyValidMultiple, txt_GuideWatchValidMultiple, txt_InquiryVaildMultiple);
                    Multiplication(txt_PropertyValidDeductScore, txt_PropertyValidMultiple);
                    Multiplication(txt_InfoValidDeductScoreSum, txt_InfoValidMultipleSum);
                    break;
                case "txt_GuideWatchValidMultiple":
                    SumTextBoxDecimal(txt_InfoValidMultipleSum, txt_PropertyValidMultiple, txt_GuideWatchValidMultiple, txt_InquiryVaildMultiple);
                    Multiplication(txt_GuideWatchValidDeductScore, txt_GuideWatchValidMultiple);
                    Multiplication(txt_InfoValidDeductScoreSum, txt_InfoValidMultipleSum);
                    break;
                case "txt_InquiryVaildMultiple":
                    SumTextBoxDecimal(txt_InfoValidMultipleSum, txt_PropertyValidMultiple, txt_GuideWatchValidMultiple, txt_InquiryVaildMultiple);
                    Multiplication(txt_InquiryVaildDeductScore, txt_InquiryVaildMultiple);
                    Multiplication(txt_InfoValidDeductScoreSum, txt_InfoValidMultipleSum);
                    break;
                //⑤被举报惩罚积分
                case "txt_BeReportedMultiple":
                    Multiplication(txt_BeReportedDeductScore, txt_BeReportedMultiple);
                    break;
                //⑥行为规范惩罚积分
                case "txt_DressUnsuitableMultiple":
                case "txt_OutWithoutRegisterMultiple":
                    SumTextBoxDecimal(txt_BehaviorStandardMultipleSum, txt_DressUnsuitableMultiple, txt_OutWithoutRegisterMultiple);
                    Multiplication(txt_BehaviorStandardDeductScoreSum, txt_BehaviorStandardMultipleSum);
                    break;
                //⑦黄线及客户投诉惩罚积分
                case "txt_YellowLineMultiple":
                case "txt_CustomerComplaintsMultiple":
                    SumTextBoxDecimal(txt_YellowAndComplaintsMultipleSum, txt_YellowLineMultiple, txt_CustomerComplaintsMultiple);
                    Multiplication(txt_YellowAndComplaintsDeductScoreSum, txt_YellowAndComplaintsMultipleSum);
                    break;
                //汇总
                case "txt_AttendanceDeductScoreSum":
                case "txt_WorkLoadLackDeductScore":
                case "txt_InfoValidDeductScoreSum":
                case "txt_BeReportedDeductScore":
                case "txt_BehaviorStandardDeductScoreSum":
                case "txt_YellowAndComplaintsDeductScoreSum":
                    SumTextBoxDecimal(txt_PunishMultipleSum, txt_AttendanceMultipleSum, txt_WorkLoadLackMultiple, txt_InfoValidMultipleSum, txt_BeReportedMultiple, txt_BehaviorStandardMultipleSum, txt_YellowAndComplaintsMultipleSum);
                    Multiplication(txt_PunishDeductScoreSum, txt_PunishMultipleSum);
                    break;
                default: break;
            }
        }

        private Tuple<bool, T> GetValueSafely<T>(string valueStr)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(valueStr))
                {
                    return new Tuple<bool, T>(true, default(T));
                }

                var result = (T)Convert.ChangeType(valueStr, typeof(T), null);
                return new Tuple<bool, T>(true, result);
            }
            catch
            {
                return new Tuple<bool, T>(false, default(T));
            }
        }

        private bool SetPropSafely<T1, T2>(T1 target, T2 value, System.Reflection.PropertyInfo prop)
        {
            try
            {
                Type genericTypeDefinition = prop.PropertyType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(Nullable<>))
                {
                    prop.SetValue(target, Convert.ChangeType(value, Nullable.GetUnderlyingType(prop.PropertyType)), null);
                }
                else
                {
                    prop.SetValue(target, Convert.ChangeType(value, prop.PropertyType), null);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void NotifyTextBox(TextBox control)
        {
            if (control == null)
            {
                return;
            }

            var oldBrush = control.BorderBrush.Clone();
            control.TextChanged += (o, e) => { control.BorderBrush = oldBrush; };
            control.BorderBrush = Brushes.Red;
        }

        private void SumTextBoxInt(TextBox sumTextBox, params TextBox[] subTextBoxs)
        {
            if (sumTextBox == null || subTextBoxs == null)
            {
                return;
            }

            int sum = 0;
            foreach (var sub in subTextBoxs)
            {
                var value = GetValueSafely<int>(sub.Text);
                if (!value.Item1)
                {
                    NotifyTextBox(sub);
                    return;
                }
                sum = sum + value.Item2;
            }
            sumTextBox.Text = sum.ToString();
        }

        private void SumTextBoxDecimal(TextBox sumTextBox, params TextBox[] subTextBoxs)
        {
            if (sumTextBox == null || subTextBoxs == null)
            {
                return;
            }

            decimal sum = 0;
            foreach (var sub in subTextBoxs)
            {
                var value = GetValueSafely<decimal>(sub.Text);
                if (!value.Item1)
                {
                    NotifyTextBox(sub);
                    return;
                }
                sum = sum + value.Item2;
            }
            sumTextBox.Text = sum.ToString();
        }

        private void Multiplication(TextBox targetTextBox, TextBox sourceTextBox)
        {
            if (BaseScore < 0 || targetTextBox == null || sourceTextBox == null)
            {
                return;
            }

            var value = GetValueSafely<decimal>(sourceTextBox.Text);
            if (!value.Item1)
            {
                NotifyTextBox(sourceTextBox);
                return;
            }
            targetTextBox.Text = (value.Item2 * BaseScore).ToString();
        }
    }

    public class LoopUpdate
    {
        public int levelID { get; set; } //对应奖惩项节点
        public string levelName { get; set; } //对应奖惩项名称
        public string PunishResult { get; set; } //处罚结果
        public int Type { get; set; } //奖惩类型

        public List<PropBind> propBingList { get; set; } //字段与输入框的对应关系
    }
    public class PropBind
    {
        public string txtBoxName { get; set; }
        public string propName { get; set; }
        public int valueType { get; set; } //1代表整数，2代表小数
    }
}
