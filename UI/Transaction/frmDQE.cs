using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WPM.BusinessObjects;
using WPM.BusinessLayer;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors;

namespace WPM
{
    public partial class frmDQE : Form
    {
        public int m_iRegId = 0;
        public string m_sMode = "";

        string m_sVoucherType = "";
        BsfGlobal.VoucherType oVType;
        BsfGlobal.VoucherType oVCCType;
        BsfGlobal.VoucherType oVCompanyType;

        public bool m_bViewScreen = false;

        DQEMasterBO oDQEReg;

        DataTable m_tIOWTrans = new DataTable();

        DataTable m_tWBSRights = new DataTable();
        string m_sSplit = "";
        string m_sProjDBName = "";

        bool m_bWOCCReqd = false;
        bool m_bWOCompReqd = false;
        int m_iCCId = 0;
        int m_iRevId = 0;
        int m_iCompanyId = 0;
        DateTime m_dEntryDate;
        DataTable m_tReg = new DataTable();

        List<DQEIOWTransBO> oDPEIOWTrans =  new List<DQEIOWTransBO>();
        List<DQEWBSTrans> oDQEWBSTrans = new List<DQEWBSTrans>();
        List<DQEMeasurementBOUpdate> oDQEMsrmnt = new  List<DQEMeasurementBOUpdate>();

        frmIOWDet frmIOWdet = new frmIOWDet();

        DQEBL oDQE;

        public frmDQE()
        {
            InitializeComponent();
            oDQEReg = new DQEMasterBO();
            oDQE = new DQEBL();
        }
        public void Execute(string argMode,int argRegId)
        {
            m_sMode = argMode;
            m_iRegId = argRegId;
            this.Show();
        }

        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private void frmDQE_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_sMode == "E")
            {
                BsfGlobal.ClearUserUsage("DQE-Entry-Modify", m_iRegId, BsfGlobal.g_sWPMDBName);
            }

            if (BsfGlobal.g_bWorkFlow == true)
            {
                if (m_sMode == "E")
                {
                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        this.Parent.Controls.Owner.Hide();
                    }
                    catch
                    {
                    }
                    ChangeGridValue(m_iRegId);
                    frmDQERegister.m_oDW.Show();
                    frmDQERegister.m_oDW.Select();
                    Cursor.Current = Cursors.Default;
                }
                else
                    try { this.Parent.Controls.Owner.Hide(); }
                    catch { }
            }
            else
            {
                if (m_sMode == "E")
                {
                    CommFun.DW1.Show();
                    CommFun.RP1.Controls.Clear();
                }
                CommFun.DW1.Hide();
                CommFun.RP1.Controls.Clear();
            }
        }

        private void ChangeGridValue(int argRegId)
        {
            DataTable dt = new DataTable();
            dt = oDQE.GetDQERegisterDetails(m_iRegId);
            int iRowId = frmDQERegister.m_oGridView.FocusedRowHandle;
            if (dt.Rows.Count > 0)
            {
                frmDQERegister.m_oGridView.SetRowCellValue(iRowId, "DQE Date", Convert.ToDateTime(dt.Rows[0]["DQE Date"]));
                frmDQERegister.m_oGridView.SetRowCellValue(iRowId, "DQE No", dt.Rows[0]["DQE No"].ToString());
                frmDQERegister.m_oGridView.SetRowCellValue(iRowId, "CCDQENo", dt.Rows[0]["CCDQENo"]);
                frmDQERegister.m_oGridView.SetRowCellValue(iRowId, "CompanyVNo", dt.Rows[0]["CompanyVNo"].ToString());
                frmDQERegister.m_oGridView.SetRowCellValue(iRowId, "CostCentreName", dt.Rows[0]["CostCentreName"].ToString());
                frmDQERegister.m_oGridView.SetRowCellValue(iRowId, "From Date", Convert.ToDateTime(dt.Rows[0]["From Date"]));
                frmDQERegister.m_oGridView.SetRowCellValue(iRowId, "To Date", Convert.ToDateTime(dt.Rows[0]["To Date"]));
                frmDQERegister.m_oGridView.SetRowCellValue(iRowId, "UserName", dt.Rows[0]["UserName"].ToString());
                frmDQERegister.m_oGridView.SetRowCellValue(iRowId, "Ready", Convert.ToBoolean(dt.Rows[0]["Ready"]));
                frmDQERegister.m_oGridView.SetRowCellValue(iRowId, "App", dt.Rows[0]["App"].ToString());
            }
            frmDQERegister.m_oGridView.FocusedRowHandle = iRowId;
            dt.Dispose();
        }

        private void frmDQE_Load(object sender, EventArgs e)
        {
            PopulateCostCentre();
            PopulateUser();

            BsfGlobal.Get_ServerDate();
            dtpDPEDate.EditValue = BsfGlobal.g_dServerDate;
            if (BsfGlobal.FindPermission("General-Allow-Other-than-Current-Date-Entries") == false) { dtpDPEDate.Enabled = false; }    

            dtpDPEDate.EditValue = BsfGlobal.g_dServerDate;
            dtpFrmDate.EditValue = BsfGlobal.g_dServerDate;
            dtpToDate.EditValue = BsfGlobal.g_dServerDate;
            txtNote.Text = "";
            GetVoucherNo();
            AddNewIOWEntry();
            xtraTabControl1.SelectedTabPage = xtraTabPage1;
            xtraTabPage2.PageVisible = false;
            xtraTabPage3.PageVisible = false;

            if (m_bViewScreen == true)
            {
                barEditItem1.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                barButtonItem1.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                barButtonItem2.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                barButtonItem3.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            }


            barEditItem1.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
            BsfGlobal.GetAutoReady();
            barEditItem1.EditValue = BsfGlobal.g_bAutoReady;
            barEditItem1.Enabled = true;

            if (m_sMode == "E")
            {
                PopulateEditData();
                BsfGlobal.InsertUserUsage("DQE-Entry-Modify", m_iRegId, BsfGlobal.g_sWPMDBName);
            }

            if (m_sMode == "A")
            {

                if (BsfGlobal.FindPermission("DQE-Entry-Create") == false)
                {
                    barButtonItem2.Enabled = false;
                }
            }
            else
            {

                if (BsfGlobal.FindPermission("DQE-Entry-Modify") == false)
                {
                    barButtonItem2.Enabled = false;
                }
            }


        }
        private void PopulateEditData()
        {
            m_tReg = new DataTable();

            m_tReg = oDQE.GetDQEEntry(m_iRegId);
            if (m_tReg.Rows.Count > 0)
            {

                m_sProjDBName = CommFun.GetProjectDB(Convert.ToInt32(m_tReg.Rows[0]["CostCentreId"].ToString()));
                m_iRevId = 0;
                if (BsfGlobal.CheckDBFound(m_sProjDBName) == true)
                {
                    m_iRevId = CommFun.GetRevisionId(m_sProjDBName);
                }
                m_sSplit = CommFun.GetWBSReqd(Convert.ToInt32(CommFun.IsNullCheck(m_tReg.Rows[0]["CostCentreId"], CommFun.datatypes.vartypenumeric)));
                m_tWBSRights = new DataTable();
                m_tWBSRights = BsfGlobal.GetWBSRights(BsfGlobal.g_lUserId, m_sProjDBName);

                cboCostcentre.EditValue = Convert.ToInt32(CommFun.IsNullCheck(m_tReg.Rows[0]["CostCentreId"], CommFun.datatypes.vartypenumeric));
                
                dtpDPEDate.EditValue = Convert.ToDateTime(m_tReg.Rows[0]["EDate"]);
                dtpFrmDate.EditValue = Convert.ToDateTime(m_tReg.Rows[0]["FDate"]);
                dtpToDate.EditValue = Convert.ToDateTime(m_tReg.Rows[0]["TDate"]);

                cboUser.EditValue = Convert.ToInt32(CommFun.IsNullCheck(m_tReg.Rows[0]["UserId"], CommFun.datatypes.vartypenumeric));

                txtCompBVNo.Text = m_tReg.Rows[0]["CompanyVNo"].ToString();
                txtLVNo.Text = m_tReg.Rows[0]["VNo"].ToString();
                txtCCDPENo.Text = m_tReg.Rows[0]["CCDQENo"].ToString();
                txtNote.Text = CommFun.IsNullCheck(m_tReg.Rows[0]["Narration"], CommFun.datatypes.vartypestring).ToString();

                m_dEntryDate = Convert.ToDateTime(m_tReg.Rows[0]["EDate"].ToString());

                if (m_tReg.Rows[0]["Approve"].ToString() == "Y" || m_tReg.Rows[0]["Approve"].ToString() == "P")
                {
                    barButtonItem2.Enabled = false;
                    barEditItem1.EditValue = true;
                    barEditItem1.Enabled = false;
                }
                else
                {
                    if (Convert.ToBoolean(m_tReg.Rows[0]["Ready"]) == true)
                    {
                        barEditItem1.EditValue = true;
                        barEditItem1.Enabled = false;
                    }
                }

            }

            AddNewIOWEntry();

            int iRowId = 0;
            DataRow dRow;
            DataSet dsIOW = new DataSet();
            dsIOW = oDQE.GetDQETrans(m_iRegId, m_sProjDBName);

            DataTable dt = new DataTable();
            dt = dsIOW.Tables[0];

            DataTable dtT = new DataTable();
            DataView dv = new DataView();

            DataTable dtWBS = new DataTable();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dRow = m_tIOWTrans.NewRow();
                iRowId = GetMaxRowId(m_tIOWTrans);
                dRow["RowId"] = iRowId;
                dRow["IOWId"] = Convert.ToInt32(dt.Rows[i]["IOWId"]);
                dRow["SerialNo"] = CommFun.IsNullCheck(dt.Rows[i]["RefSerialNo"], CommFun.datatypes.vartypestring).ToString();
                dRow["Specification"] = CommFun.IsNullCheck(dt.Rows[i]["Specification"], CommFun.datatypes.vartypestring).ToString();
                dRow["Unit"] = CommFun.IsNullCheck(dt.Rows[i]["Unit"], CommFun.datatypes.vartypestring).ToString();
                dRow["Qty"] = Convert.ToDecimal(CommFun.IsNullCheck(dt.Rows[i]["Qty"], CommFun.datatypes.vartypenumeric));
                dRow["PrevQty"] = Convert.ToDecimal(CommFun.IsNullCheck(dt.Rows[i]["Qty"], CommFun.datatypes.vartypenumeric));

                m_tIOWTrans.Rows.Add(dRow);

                if (m_sSplit == "Y")
                {
                    dtWBS = new DataTable();
                    dtWBS = CommFun.GetWOWBS(m_iRevId, Convert.ToInt32(dt.Rows[i]["IOWId"]), m_sSplit, m_sProjDBName);
                    dv = new DataView(dsIOW.Tables[1]);
                    dv.RowFilter = "DQETransId = " + Convert.ToInt32(dt.Rows[i]["DQETransId"]);
                    dtT = new DataTable();
                    dtT = dv.ToTable();
                    dv.Dispose();

                    PopulateDPEWBSAdd(iRowId, Convert.ToInt32(dt.Rows[i]["IOWId"]),dtWBS,dtT);
                }

            }


        }

        private void PopulateCostCentre()
        {
            DataSet ds = new DataSet();
            ds = CommFun.PopulateCostCentreVendor(m_sMode);
            DataTable dt = new DataTable();
            dt = ds.Tables["CostCentre"];
            DataRow dr;
            dr = dt.NewRow();
            dr["CostCentreName"] = "None";
            dr["CostCentreId"] = 0;
            dt.Rows.InsertAt(dr, 0);
            cboCostcentre.Properties.DataSource = dt;
            cboCostcentre.Properties.PopulateColumns();
            cboCostcentre.Properties.DisplayMember = "CostCentreName";
            cboCostcentre.Properties.ValueMember = "CostCentreId";
            cboCostcentre.Properties.Columns["CostCentreId"].Visible = false;
            cboCostcentre.Properties.Columns["HO"].Visible = false;
            cboCostcentre.Properties.ShowFooter = false;
            cboCostcentre.Properties.ShowHeader = false;
            cboCostcentre.EditValue = 0;
        }

        private void PopulateUser()
        {
            DataTable dt = new DataTable();
            dt = CommFun.GetUsers();
            cboUser.Properties.DataSource = dt;
            cboUser.Properties.PopulateColumns();
            cboUser.Properties.DisplayMember = "UserName";
            cboUser.Properties.ValueMember = "UserId";
            cboUser.Properties.Columns["UserId"].Visible = false;
            cboUser.Properties.ShowFooter = false;
            cboUser.Properties.ShowHeader = false;
            cboUser.EditValue = BsfGlobal.g_lUserId;
        }

        private void GetVoucherNo()
        {
            m_sVoucherType = BsfGlobal.GetVoucherType(96);

            oVType = new BsfGlobal.VoucherType();
            oVType = BsfGlobal.GetVoucherNo(96, Convert.ToDateTime(dtpDPEDate.EditValue), 0, 0);


            BsfGlobal.CheckVoucherType(96, ref m_bWOCCReqd, ref m_bWOCompReqd);

            //if (m_bWOCCReqd == false)
            //{
            //    txtCCDPENo.Visible = false;
            //    lblCCDPENo.Visible = false;
            //}
            //else
            //{
            //    txtCCDPENo.Visible = true;
            //    lblCCDPENo.Visible = true;
            //}

            //if (m_bWOCompReqd == false)
            //{
            //    txtCompBVNo.Visible = false;
            //    lblCompNo.Visible = false;
            //}
            //else
            //{
            //    txtCompBVNo.Visible = true;
            //    lblCompNo.Visible = true;
            //}


            if (oVType.GenType == true)
            {
                if (m_sVoucherType == "  " || m_sVoucherType == "GE")
                {
                    txtLVNo.Text = oVType.VoucherNo;
                    barStaticItem1.Caption = oVType.VoucherNo;
                    txtLVNo.Visible = false;
                    lblDPENo.Visible = false;
                }
                else
                {
                    txtLVNo.Enabled = false;
                    txtLVNo.Visible = true;
                    lblDPENo.Visible = true;
                    txtLVNo.Text = oVType.VoucherNo;
                }
            }
            else
            {
                txtLVNo.Enabled = true;
                lblDPENo.Visible = true;
                barStaticItem1.Caption = "";
                txtLVNo.Text = "";
            }
        }


        private void cboCostcentre_EditValueChanged(object sender, EventArgs e)
        {
            if (Convert.ToInt32(cboCostcentre.EditValue) == 0) { return; }
            if (Convert.ToInt32(cboCostcentre.EditValue) != 0)
            {
                m_iCCId = Convert.ToInt32(cboCostcentre.EditValue);
                m_sProjDBName = CommFun.GetProjectDB(m_iCCId);
                m_iRevId = 0;
                m_tWBSRights = new DataTable();

                if (BsfGlobal.CheckDBFound(m_sProjDBName) == true)
                {
                    m_iRevId = CommFun.GetRevisionId(m_sProjDBName);
                }
                else
                {
                    MessageBox.Show("Project DataBase Not Linked in this CostCentre...");
                    cboCostcentre.EditValue = 0;
                    return;
                }

                m_tWBSRights = BsfGlobal.GetWBSRights(BsfGlobal.g_lUserId, m_sProjDBName);
                DataTable dtC = new DataTable();
                dtC = CommFun.GetCompanyId(m_iCCId);
                if (dtC.Rows.Count > 0)
                {
                    m_iCompanyId = Convert.ToInt32(CommFun.IsNullCheck(dtC.Rows[0]["CompanyId"], CommFun.datatypes.vartypenumeric));
                }
                m_sSplit = CommFun.GetWBSReqd(m_iCCId);
                int iFACCId = CommFun.GetFACCId2(m_iCCId);

                oVType = new BsfGlobal.VoucherType();
                oVCCType = new BsfGlobal.VoucherType();
                oVCompanyType = new BsfGlobal.VoucherType();

                if (m_sMode == "A")
                {
                    m_sVoucherType = BsfGlobal.GetVoucherType(96);
                    oVCCType = BsfGlobal.GetVoucherNo(96, Convert.ToDateTime(dtpDPEDate.EditValue), 0, Convert.ToInt32(cboCostcentre.EditValue));
                    if (oVCCType.GenType == true)
                    {
                        if (m_sVoucherType == "CC")
                        {
                            barStaticItem1.Caption = oVCCType.VoucherNo;
                            txtCCDPENo.Text = oVCCType.VoucherNo;
                            txtCCDPENo.Enabled = false;
                            lblCCDPENo.Enabled = false;
                        }
                        else
                        {
                            txtCCDPENo.Enabled = true;
                            txtCCDPENo.Text = oVCCType.VoucherNo;
                            txtCCDPENo.Visible = true;
                            lblCCDPENo.Visible = true;
                        }
                    }
                    else
                    {
                        txtCCDPENo.Enabled = true;
                        txtCCDPENo.Text = "";
                    }
                    oVCompanyType = BsfGlobal.GetVoucherNo(96, Convert.ToDateTime(dtpDPEDate.EditValue), m_iCompanyId, 0);
                    if (oVCompanyType.GenType == true)
                    {
                        if (m_sVoucherType == "CO")
                        {
                            barStaticItem1.Caption = oVCompanyType.VoucherNo;
                            txtCompBVNo.Text = oVCompanyType.VoucherNo;
                            txtCompBVNo.Enabled = false;
                            lblCompNo.Enabled = false;
                        }
                        else
                        {
                            txtCompBVNo.Enabled = false;
                            txtCompBVNo.Text = oVCompanyType.VoucherNo;
                            txtCompBVNo.Visible = true;
                            lblCompNo.Visible = true;
                        }
                    }
                    else
                    {
                        txtCompBVNo.Enabled = true;
                        txtCompBVNo.Text = "";
                    }
                }
                if (m_sMode == "E")
                {
                    if (m_sVoucherType == "  " || m_sVoucherType == "GE")
                    {
                        lblDPENo.Enabled = false;
                        txtLVNo.Enabled = false;
                        barStaticItem1.Caption = m_tReg.Rows[0]["VNo"].ToString();
                        txtLVNo.Text = m_tReg.Rows[0]["VNo"].ToString();
                    }
                    else
                    {
                        lblDPENo.Visible = true;
                        txtLVNo.Visible = true;
                        txtLVNo.Text = m_tReg.Rows[0]["VNo"].ToString();
                        txtLVNo.Enabled = false;
                    }
                    if (m_sVoucherType == "CO")
                    {
                        lblCompNo.Enabled = false;
                        txtCompBVNo.Enabled = false;
                        txtCompBVNo.Text = m_tReg.Rows[0]["CompanyVNo"].ToString();
                        barStaticItem1.Caption = m_tReg.Rows[0]["CompanyVNo"].ToString();
                    }
                    else
                    {
                        lblCompNo.Visible = true;
                        txtCompBVNo.Visible = true;
                        txtCompBVNo.Text = m_tReg.Rows[0]["CompanyVNo"].ToString();
                        txtCompBVNo.Enabled = false;
                    }
                    if (m_sVoucherType == "CC")
                    {
                        lblCCDPENo.Enabled = false;
                        txtCCDPENo.Enabled = false;
                        txtCCDPENo.Text = m_tReg.Rows[0]["CCDQENo"].ToString();
                        barStaticItem1.Caption = m_tReg.Rows[0]["CCDQENo"].ToString();
                    }
                    else
                    {
                        lblCCDPENo.Visible = true;
                        txtCCDPENo.Visible = true;
                        txtCCDPENo.Text = m_tReg.Rows[0]["CCDQENo"].ToString();
                        txtCCDPENo.Enabled = false;
                    }

                }
            }
        }

        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (Convert.ToInt32(cboCostcentre.EditValue) <=0)
            {
                MessageBox.Show("Select CostCentre", "WPM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cboCostcentre.Focus();
                return;
            }

            frmComponent oCompnt = new frmComponent();
            string sIOWTransId = GetIOWTransIds();
            DataTable dt = new DataTable();
            
            dt = oCompnt.Execute("D", "I", "", sIOWTransId, "", m_iCCId, 0, m_iRevId, 0, "", "", m_sProjDBName, 0, 0);
            if (dt != null)
            {
                InsertIOWTrans(dt);
            }
        }
        private void InsertIOWTrans(DataTable argDt)
        {

            DataRow dRow;
            int iRowId = 0;
            DataTable dtWBS;
            DataTable dt = new DataTable();
            for (int i=0; i < argDt.Rows.Count; i++)
            {
                dRow = m_tIOWTrans.NewRow();
                iRowId = GetMaxRowId(m_tIOWTrans);
                dRow["RowId"] = iRowId;
                dRow["IOWId"] = Convert.ToInt32(argDt.Rows[i]["IOW_ID"]);
                dRow["SerialNo"] = CommFun.IsNullCheck(argDt.Rows[i]["ResourceCode"], CommFun.datatypes.vartypestring).ToString();
                dRow["Specification"] = CommFun.IsNullCheck(argDt.Rows[i]["Description"], CommFun.datatypes.vartypestring).ToString();
                dRow["Unit"] = CommFun.IsNullCheck(argDt.Rows[i]["Unit"], CommFun.datatypes.vartypestring).ToString();
                dRow["Qty"] = 0;
                dRow["PrevQty"] = 0;

                m_tIOWTrans.Rows.Add(dRow);

                if (m_sSplit == "Y")
                {
                    dtWBS = new DataTable();
                    dtWBS = CommFun.GetWOWBS(m_iRevId, Convert.ToInt32(argDt.Rows[i]["IOW_ID"]), m_sSplit, m_sProjDBName);
                    PopulateDPEWBSAdd(iRowId, Convert.ToInt32(argDt.Rows[i]["IOW_ID"]),dtWBS,dt);
                }
            }
            if (m_tIOWTrans.Rows.Count > 0) { cboCostcentre.Enabled = false; }
            else { cboCostcentre.Enabled = true; }
        }


        private void PopulateDPEWBSAdd(int argRowId, int argIOWId,DataTable argDt,DataTable argDQEWBS)
        {
            try
            {
                if (oDQEWBSTrans.Count > 0)
                {
                    List<DQEWBSTrans> ocheckUpdate = oDQEWBSTrans.FindAll(
                        delegate(DQEWBSTrans sel)
                        {
                            return (sel.RowId == argRowId);
                        });
                    if (ocheckUpdate.Count > 0)
                    {
                        oDQEWBSTrans.RemoveAll(
                            delegate(DQEWBSTrans sel)
                            {
                                return (sel.RowId == argRowId);
                            });
                    }
                }

                DataTable dt = new DataTable();
                DataView dv;
                for (int k = 0; k < argDt.Rows.Count; k++)
                {

                    DataRow[] customerRow = argDt.Select("Analysis_ID=" + Convert.ToInt32(CommFun.IsNullCheck(argDt.Rows[k]["Analysis_ID"], CommFun.datatypes.vartypenumeric)));

                    if (argDQEWBS.Rows.Count > 0)
                    {

                        dv = new DataView(argDQEWBS);
                        dv.RowFilter = "AnalHeadId = " + Convert.ToInt32(CommFun.IsNullCheck(argDt.Rows[k]["Analysis_ID"], CommFun.datatypes.vartypenumeric));
                        dt = new DataTable();
                        dt = dv.ToTable();
                        dv.Dispose();
                        if (dt.Rows.Count > 0)
                        {
                            customerRow[0]["Qty"] = Convert.ToDecimal(dt.Rows[0]["Qty"]);
                        }
                        else
                        {
                            customerRow[0]["Qty"] = 0;
                        }
                        dt.Dispose();
                    }
                    else
                    {
                        customerRow[0]["Qty"] = 0;
                    }

                    oDQEWBSTrans.Add(new DQEWBSTrans()
                    {
                        WBSRowId = k+1,
                        RowId = argRowId,
                        AnalysisHeadId = Convert.ToInt32(CommFun.IsNullCheck(argDt.Rows[k]["Analysis_ID"], CommFun.datatypes.vartypenumeric)),
                        Level3 = CommFun.IsNullCheck(argDt.Rows[k]["Level3"], CommFun.datatypes.vartypestring).ToString(),
                        Level2 = CommFun.IsNullCheck(argDt.Rows[k]["Level2"], CommFun.datatypes.vartypestring).ToString(),
                        Level1 = CommFun.IsNullCheck(argDt.Rows[k]["Level1"], CommFun.datatypes.vartypestring).ToString(),
                        ResourceCode = CommFun.IsNullCheck(argDt.Rows[k]["RefSerialNo"], CommFun.datatypes.vartypestring).ToString(),
                        Description = CommFun.IsNullCheck(argDt.Rows[k]["AnalysisHeadName"], CommFun.datatypes.vartypestring).ToString(),
                        Qty = Convert.ToInt32(CommFun.IsNullCheck(argDt.Rows[k]["Qty"], CommFun.datatypes.vartypenumeric)),
                        PrevQty = Convert.ToInt32(CommFun.IsNullCheck(argDt.Rows[k]["Qty"], CommFun.datatypes.vartypenumeric))
                    });
                }
            }
            catch (Exception ex)
            {
                BsfGlobal.CustomException(ex.Message, ex.StackTrace);
            }
        }


        private int GetMaxRowId(DataTable argdt)
        {
            int iMaxId = 0;
            if (argdt.Rows.Count > 0)
            {
                iMaxId = Convert.ToInt32(argdt.Compute("Max(RowId)", ""));
                iMaxId = iMaxId + 1;
            }
            else
            {
                iMaxId = 1;
            }
            return iMaxId;
        }
        private string GetIOWTransIds()
        {
            string sIOWs = "";
            for (int i = 0; i < gridView1.RowCount; i++)
            {
                sIOWs = sIOWs + CommFun.IsNullCheck(gridView1.GetRowCellValue(i, "IOWId").ToString(), CommFun.datatypes.vartypenumeric).ToString() + ",";
            }
            if (sIOWs != "") { sIOWs = sIOWs.TrimEnd(','); }

            return sIOWs;
        }

        private void barButtonItem6_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (Convert.ToInt32(cboCostcentre.EditValue) <= 0)
            {
                MessageBox.Show("Select CostCentre", "WPM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cboCostcentre.Focus();
                return;
            }

        }

        private void barButtonItem8_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (Convert.ToInt32(cboCostcentre.EditValue) <= 0)
            {
                MessageBox.Show("Select CostCentre", "WPM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cboCostcentre.Focus();
                return;
            }
        }

        private void AddNewIOWEntry()
        {
            if (m_tIOWTrans.Columns.Count > 0) { return; }

            m_tIOWTrans.Columns.Add("RowId", typeof(int));
            m_tIOWTrans.Columns.Add("IOWId", typeof(int));
            m_tIOWTrans.Columns.Add("SerialNo", typeof(string));
            m_tIOWTrans.Columns.Add("Specification", typeof(string));
            m_tIOWTrans.Columns.Add("Unit", typeof(string));
            m_tIOWTrans.Columns.Add("Qty", typeof(decimal));
            m_tIOWTrans.Columns.Add("PrevQty", typeof(decimal));

            gridControl1.DataSource = m_tIOWTrans;

            RepositoryItemButtonEdit btnQty = new RepositoryItemButtonEdit();
            btnQty.LookAndFeel.SkinName = "Blue";
            btnQty.LookAndFeel.UseDefaultLookAndFeel = false;
            btnQty.Mask.EditMask = BsfGlobal.g_sQtyDigitFormat;
            btnQty.DisplayFormat.FormatString = BsfGlobal.g_sQtyDigitFormat;
            btnQty.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            btnQty.Mask.UseMaskAsDisplayFormat = true;
            btnQty.Validating += new CancelEventHandler(btnQty_Validating);
            btnQty.KeyPress += new KeyPressEventHandler(btnQty_KeyPress);
            btnQty.DoubleClick += new EventHandler(btnQty_DoubleClick);
            btnQty.Leave += new EventHandler(btnQty_Leave);
            btnQty.Spin += new DevExpress.XtraEditors.Controls.SpinEventHandler(btnQty_Spin);
            btnQty.KeyDown += new KeyEventHandler(btnQty_KeyDown);

            gridView1.Columns["Qty"].ColumnEdit = btnQty;
            gridView1.Columns["RowId"].Visible = false;
            gridView1.Columns["IOWId"].Visible = false;
            gridView1.Columns["PrevQty"].Visible = false;
            gridView1.Columns["SerialNo"].Width = 100;
            gridView1.Columns["Specification"].Width = 300;
            gridView1.Columns["Unit"].Width = 70;
            gridView1.Columns["Qty"].Width = 100;

            gridView1.Columns["SerialNo"].OptionsColumn.AllowEdit = false;
            gridView1.Columns["Specification"].OptionsColumn.AllowEdit = false;
            gridView1.Columns["Unit"].OptionsColumn.AllowEdit = false;

        }

        void btnQty_Validating(object sender, CancelEventArgs e)
        {
            ButtonEdit editor = (ButtonEdit)sender;

            decimal dVarQty = 0;
            if (BsfGlobal.FindPermissionVariant("Allow-DQE-Entry-Qty-Greater-than-Estimate-Qty", ref dVarQty) == false)
            {
            }

            decimal dEstimate = Convert.ToDecimal(CommFun.IsNullCheck(gridView2.GetRowCellValue(0, "Estimate"), CommFun.datatypes.vartypenumeric));
            dEstimate = dEstimate * (1 + dVarQty / 100);
            decimal dWorkDone = Convert.ToDecimal(CommFun.IsNullCheck(gridView2.GetRowCellValue(0, "WorkDone"), CommFun.datatypes.vartypenumeric));
            decimal dPrevQty = Convert.ToDecimal(CommFun.IsNullCheck(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "PrevQty"), CommFun.datatypes.vartypenumeric));
            decimal dCurQty = Convert.ToDecimal(CommFun.IsNullCheck(editor.EditValue, CommFun.datatypes.vartypenumeric));

            if (dCurQty > dEstimate - dWorkDone + dPrevQty)
            {
                MessageBox.Show("Qty Greater than Estimate Qty");
                gridView1.SetRowCellValue(gridView1.FocusedRowHandle, "Qty", 0);
                gridView1.UpdateCurrentRow();
                return;
            }

            gridView1.SetRowCellValue(gridView1.FocusedRowHandle, "Qty", dCurQty);
            gridView1.UpdateCurrentRow();
        }

        void btnQty_DoubleClick(object sender, EventArgs e)
        {
            ButtonEdit editor = (ButtonEdit)sender;

            DataTable dtIAmtMsr = new DataTable();
            int iIOWId = Convert.ToInt32(CommFun.IsNullCheck(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "IOWId"), CommFun.datatypes.vartypenumeric));
            string sUnit = CommFun.IsNullCheck(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "Unit"), CommFun.datatypes.vartypenumeric).ToString();
            int iRowId = Convert.ToInt32(CommFun.IsNullCheck(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "RowId").ToString(), CommFun.datatypes.vartypenumeric));
            string sDescription = CommFun.IsNullCheck(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "Specification"), CommFun.datatypes.vartypestring).ToString();

            if (m_sSplit == "Y")
            {

                List<DQEWBSTrans> ocheckUpdate = oDQEWBSTrans.FindAll(
                delegate(DQEWBSTrans sel)
                {
                    return (sel.RowId == iRowId);

                });

                DataTable dt1 = new DataTable();
                DataTable dt2 = new DataTable();
                DataTable dt3 = new DataTable();


                frmIOWdet.Execute("Q", m_iCCId, 0, m_iRevId, iIOWId, CommFun.GenericListToDataTable(ocheckUpdate), CommFun.GenericListToDataTable(oDQEMsrmnt), sDescription, m_sSplit, ref dt1, ref dt2, ref dt3, m_sProjDBName, 0, 0, iRowId, m_tWBSRights, Convert.ToDateTime(dtpDPEDate.EditValue));
                if (frmIOWdet.m_sClkOption == "OK")
                {
                    gridView1.SetRowCellValue(gridView1.FocusedRowHandle, "Qty", frmIOWdet.m_dRetruntQty);
                    DPEWBSUpdateIOW(iRowId);
                }
            }

        }

        private void DPEWBSUpdateIOW(int argRowId)
        {
            try
            {
                if (frmIOWdet.grdViewIDet.RowCount > 0)
                {
                    if (oDQEWBSTrans.Count > 0)
                    {
                        oDQEWBSTrans.RemoveAll(
                        delegate(DQEWBSTrans del)
                        {
                            return (del.RowId == argRowId);

                        });
                    }
                    for (int q = 0; q < frmIOWdet.grdViewIDet.RowCount; q++)
                    {
                        oDQEWBSTrans.Add(new DQEWBSTrans()
                        {
                            WBSRowId = Convert.ToInt32(CommFun.IsNullCheck(frmIOWdet.grdViewIDet.GetRowCellValue(q, "RowId"), CommFun.datatypes.vartypenumeric)),
                            RowId = argRowId,
                            Level3 = CommFun.IsNullCheck(frmIOWdet.grdViewIDet.GetRowCellValue(q, "Level3"), CommFun.datatypes.vartypestring).ToString(),
                            Level2 = CommFun.IsNullCheck(frmIOWdet.grdViewIDet.GetRowCellValue(q, "Level2"), CommFun.datatypes.vartypestring).ToString(),
                            Level1 = CommFun.IsNullCheck(frmIOWdet.grdViewIDet.GetRowCellValue(q, "Level1"), CommFun.datatypes.vartypestring).ToString(),
                            AnalysisHeadId = Convert.ToInt32(CommFun.IsNullCheck(frmIOWdet.grdViewIDet.GetRowCellValue(q, "AnalysisHeadId"), CommFun.datatypes.vartypenumeric)),
                            ResourceCode = CommFun.IsNullCheck(frmIOWdet.grdViewIDet.GetRowCellValue(q, "ResourceCode"), CommFun.datatypes.vartypestring).ToString(),
                            Description = CommFun.IsNullCheck(frmIOWdet.grdViewIDet.GetRowCellValue(q, "AnalysisHeadName"), CommFun.datatypes.vartypestring).ToString(),
                            Qty = Convert.ToDecimal(CommFun.IsNullCheck(frmIOWdet.grdViewIDet.GetRowCellValue(q, "Qty"), CommFun.datatypes.vartypenumeric)),
                            PrevQty = Convert.ToDecimal(CommFun.IsNullCheck(frmIOWdet.grdViewIDet.GetRowCellValue(q, "PrevQty"), CommFun.datatypes.vartypenumeric))
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                BsfGlobal.CustomException(ex.Message, ex.StackTrace);
            }

        }

        void btnQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsNumber(e.KeyChar) & (Keys)e.KeyChar != Keys.Back & e.KeyChar != '.' & e.KeyChar == '-')
            {
                e.Handled = true;
                return;
            }

            ButtonEdit editor = (ButtonEdit)sender;
            if (m_sSplit == "Y")
            {
                editor.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                editor.Properties.ReadOnly = true;

                btnQty_DoubleClick(sender, e);
            }
            else
            {
                //int iRowId = Convert.ToInt32(CommFun.IsNullCheck(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "RowId"), CommFun.datatypes.vartypenumeric));

                //if (oDQEMsrmnt.Count > 0)
                //{
                //    List<DQEMeasurementBOUpdate> oSelMsr = oDQEMsrmnt.FindAll(
                //        delegate(DQEMeasurementBOUpdate sel)
                //        {
                //            return (sel.TransRowId == iRowId);
                //        });
                //    if (oSelMsr.Count > 0)
                //    {
                //        editor.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                //        editor.Properties.ReadOnly = true;
                //        btnQty_DoubleClick(sender, e);
                //        return;
                //    }
                //}
            }
        }

        void btnQty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up | e.KeyCode == Keys.Down)
            {
                e.Handled = true;
            }
        }

        void btnQty_Leave(object sender, EventArgs e)
        {

            ButtonEdit editor = (ButtonEdit)sender;
            if (editor.IsModified == true)
            {
                editor.Focus();
                return;
            }
        }

        void btnQty_Spin(object sender, DevExpress.XtraEditors.Controls.SpinEventArgs e)
        {
            e.Handled = true;
        }

        private void barButtonItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (gridView1.FocusedRowHandle < 0) { return; }

            DialogResult reply = MessageBox.Show("Do you want Delete?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (reply == DialogResult.Yes)
            {

                int iRowId = Convert.ToInt32(CommFun.IsNullCheck(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "RowId"), CommFun.datatypes.vartypenumeric));

                if (oDQEWBSTrans.Count > 0)
                {
                    List<DQEWBSTrans> oSelect = oDQEWBSTrans.FindAll(
                        delegate(DQEWBSTrans dl)
                        {
                            return dl.RowId == iRowId;
                        });
                    if (oSelect.Count > 0)
                    {
                        oDQEWBSTrans.RemoveAll(
                        delegate(DQEWBSTrans dl)
                        {
                            return dl.RowId == iRowId;
                        });
                    }
                }

                gridView1.DeleteRow(gridView1.FocusedRowHandle);
                m_tIOWTrans.AcceptChanges();

                if (m_tIOWTrans.Rows.Count > 0) { cboCostcentre.Enabled = false; }
                else { cboCostcentre.Enabled = true; }
            }
        }

        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            UpdateData();
        }
        private void UpdateData()
        {

            dtpDPEDate.DoValidate();
            dtpFrmDate.DoValidate();
            dtpToDate.DoValidate();

            if (gridView1.RowCount == 0)
            {
                MessageBox.Show("There is No Item", "WPM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (BsfGlobal.FindPermission("General-Allow-Future-Date-Entries") == false)
            {
                BsfGlobal.Get_ServerDate();
                if (Convert.ToDateTime(Convert.ToDateTime(dtpDPEDate.EditValue).ToString("dd MMM yyyy")) > BsfGlobal.g_dServerDate)
                {
                    MessageBox.Show("DQE Date is Greater than Current Date. Cannot Proceed...");
                    dtpDPEDate.Focus();
                    return;
                }
            }
            

            if (BsfGlobal.FindPermission("General-Allow-Back-Date-Entries") == false)
            {
                Nullable<DateTime> dNulldate = BsfGlobal.GetFreezeBackDate(96);
                if (dNulldate != null)
                {
                    if (Convert.ToDateTime(Convert.ToDateTime(dtpDPEDate.EditValue).ToString("dd MMM yyyy")) < dNulldate)
                    {
                        string stg = "Do Not Allow Back Date Entry Before" + dNulldate.ToString();
                        MessageBox.Show(stg, "WPM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }


            oDQEReg = new DQEMasterBO();
            oDQEReg.EDate = Convert.ToDateTime(dtpDPEDate.EditValue);
            oDQEReg.FDate = Convert.ToDateTime(dtpFrmDate.EditValue);
            oDQEReg.TDate = Convert.ToDateTime(dtpToDate.EditValue);
            oDQEReg.CCVNo = txtCCDPENo.Text;
            oDQEReg.CompVNo = txtCompBVNo.Text;
            oDQEReg.VNo = txtLVNo.Text;
            oDQEReg.CostCentreId = m_iCCId;
            oDQEReg.UserId = Convert.ToInt32(CommFun.IsNullCheck(cboUser.EditValue,CommFun.datatypes.vartypenumeric));
            oDQEReg.Narration = CommFun.Insert_SingleQuot(CommFun.IsNullCheck(txtNote.EditValue, CommFun.datatypes.vartypestring).ToString());
            oDQEReg.Ready = Convert.ToBoolean(barEditItem1.EditValue);

            bool bUpdate = false;
            string sRefNo = "";
            if (m_sMode == "A")
            {
                int iRegId = 0;
                bUpdate = oDQE.InsertDQE(oDQEReg, m_tIOWTrans, oDQEWBSTrans, ref iRegId, ref sRefNo);

                if (bUpdate == true)
                {
                    if (Convert.ToBoolean(barEditItem1.EditValue) == true)
                    {
                        BsfGlobal.Get_ServerDate();
                        BsfGlobal.InsertLog(BsfGlobal.g_dServerDateTime, "DQE-Entry-Create", "N", "DQE-Entry", iRegId, m_iCCId, m_iCompanyId, BsfGlobal.g_sWPMDBName, sRefNo, BsfGlobal.g_lUserId);
                    }
                    ClearEntries();
                    cboCostcentre.Focus();
                }
            }
            else
            {
                if (m_dEntryDate != Convert.ToDateTime(dtpDPEDate.EditValue))
                {
                    if (oVType.PeriodWise == true)
                    {

                        if (BsfGlobal.CheckPeriodChange(oDQEReg.EDate, Convert.ToDateTime(dtpDPEDate.EditValue)) == true)
                        {
                            oVType = BsfGlobal.GetVoucherNo(96, Convert.ToDateTime(dtpDPEDate.EditValue), 0, 0);
                            if (oVCCType.GenType == true)
                            {
                                txtLVNo.Text = oVType.VoucherNo;
                                oDQEReg.VNo = oVType.VoucherNo;
                            }
                            oVCCType = BsfGlobal.GetVoucherNo(96, Convert.ToDateTime(dtpDPEDate.EditValue), 0, oDQEReg.CostCentreId);
                            if (oVCCType.GenType == true)
                            {
                                txtCCDPENo.Text = oVCCType.VoucherNo;
                                oDQEReg.CCVNo = oVCCType.VoucherNo;
                            }

                            oVCompanyType = BsfGlobal.GetVoucherNo(96, Convert.ToDateTime(dtpDPEDate.EditValue), oDQEReg.CompanyId, 0);
                            if (oVCompanyType.GenType == true)
                            {
                                txtCompBVNo.Text = oVCompanyType.VoucherNo;
                                oDQEReg.CompVNo = oVCompanyType.VoucherNo;
                            }

                            BsfGlobal.UpdateMaxNo(96, oVType, 0, 0);
                            BsfGlobal.UpdateMaxNo(96, oVCCType, 0, oDQEReg.CostCentreId);
                            BsfGlobal.UpdateMaxNo(96, oVCompanyType, oDQEReg.CompanyId, 0);
                        }
                    }
                }

                bUpdate = oDQE.UpdateDQE(m_iRegId, oDQEReg, m_tIOWTrans, oDQEWBSTrans);
                if (bUpdate == true)
                {
                    if (m_sVoucherType == "GE" || m_sVoucherType == "  ")
                    { sRefNo = oDQEReg.VNo; }
                    else if (m_sVoucherType == "CC")
                    { sRefNo = oDQEReg.CCVNo; }
                    else if (m_sVoucherType == "CO")
                    { sRefNo = oDQEReg.CompVNo; }


                    if (Convert.ToBoolean(barEditItem1.EditValue) == true)
                    {
                        BsfGlobal.Get_ServerDate();
                        if (barEditItem1.Enabled == true)
                        {
                            BsfGlobal.InsertLog(BsfGlobal.g_dServerDateTime, "DQE-Entry-Create", "N", "DQE-Entry", m_iRegId, m_iCCId, m_iCompanyId, BsfGlobal.g_sWPMDBName, sRefNo, BsfGlobal.g_lUserId);
                        }
                        else
                        {
                            BsfGlobal.InsertLog(BsfGlobal.g_dServerDateTime, "DQE-Entry-Modify", "E", "DQE-Entry", m_iRegId, m_iCCId, m_iCompanyId, BsfGlobal.g_sWPMDBName, sRefNo, BsfGlobal.g_lUserId);
                        }
                    }

                    this.Close();
                }
            }

        }
        private void ClearEntries()
        {
            m_tIOWTrans.Rows.Clear();
            oDQEReg = new DQEMasterBO();
            oDQEMsrmnt = new List<DQEMeasurementBOUpdate>();
            oDQEWBSTrans = new List<DQEWBSTrans>();
            txtNote.Text = "";
            cboCostcentre.Enabled = true;
            GetVoucherNo();
        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (gridView1.FocusedRowHandle < 0) { return; }
            PopulateGridDetails();
        }

        private void PopulateGridDetails()
        {
            int iIOWId = Convert.ToInt32(CommFun.IsNullCheck(gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "IOWId"), CommFun.datatypes.vartypenumeric));
            DataTable dt = new DataTable();
            dt = oDQE.GetDetails(m_iCCId, iIOWId, m_sProjDBName, m_iRevId,m_sSplit);
            gridControl2.DataSource = dt;
            gridView2.PopulateColumns();
            gridView2.Columns["Estimate"].Width = 100;
            gridView2.Columns["WorkDone"].Width = 100;
            gridView2.Columns["Balance"].Width = 100;

            gridView2.Columns["Estimate"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridView2.Columns["Estimate"].DisplayFormat.FormatString = BsfGlobal.g_sQtyDigitFormat;

            gridView2.Columns["WorkDone"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridView2.Columns["WorkDone"].DisplayFormat.FormatString = BsfGlobal.g_sQtyDigitFormat;

            gridView2.Columns["Balance"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridView2.Columns["Balance"].DisplayFormat.FormatString = BsfGlobal.g_sQtyDigitFormat;
        }

        private void gridView1_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            if (gridView1.FocusedRowHandle < 0) { return; }
            if (gridView1.RowCount != 1) { return; }
            PopulateGridDetails();
        }

        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (m_sMode == "A")
            {
                ClearEntries();
                cboCostcentre.Focus();
            }
            else { this.Close(); }
        }
    }
}
