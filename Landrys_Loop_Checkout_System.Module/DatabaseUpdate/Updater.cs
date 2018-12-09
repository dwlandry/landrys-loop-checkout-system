using System;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.Strategy;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout;
using System.Windows.Forms;
using Landrys_Loop_Checkout_System.Module.BusinessObjects;

namespace Landrys_Loop_Checkout_System.Module.DatabaseUpdate {
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppUpdatingModuleUpdatertopic.aspx
    public class Updater : ModuleUpdater {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
            base(objectSpace, currentDBVersion) {
        }
        public override void UpdateDatabaseAfterUpdateSchema() {
            base.UpdateDatabaseAfterUpdateSchema();

            #region Create Loop Status
            CreateLoopStatus(1, "Not Ready for Check");
            CreateLoopStatus(2, "Ready for Check");
            CreateLoopStatus(3, "Being Checked");
            CreateLoopStatus(4, "Engineering Problems");
            CreateLoopStatus(5, "Contractor Wiring Mods");
            CreateLoopStatus(6, "Complete but waiting on graphics repair");
            CreateLoopStatus(7, "Complete but waiting on config repair");
            CreateLoopStatus(8, "Complete but waiting on signature");
            CreateLoopStatus(9, "Complete - Ready for Startup");
            #endregion

            #region Initial setup of Job Info
            JobInfo jobInfo = ObjectSpace.FindObject<JobInfo>(CriteriaOperator.Parse("Oid>0"));
            if (jobInfo == null)
            {
                jobInfo = ObjectSpace.CreateObject<JobInfo>();
                jobInfo.Number = "Enter the Job Number";
                jobInfo.Description = "Enter the Job Description";
                jobInfo.ClientName = "Enter the Client Name";
                ObjectSpace.CommitChanges();
            }
            #endregion

            #region Create Default Control System Types
            CreateControlSystemType("DCS", "Primary digital control system");
            CreateControlSystemType("SIS", "Safety systems, triconex, etc.");
            CreateControlSystemType("Other", "Local PLC systems; manufactuer supplied PLCs");
            #endregion

            #region Create Default IO Types
            CreateIOType("AI");
            CreateIOType("AO");
            CreateIOType("DI");
            CreateIOType("DO");
            #endregion

            #region Create Schedules
            CreateSchedule("Original");
            CreateSchedule("Current");
            CreateSchedule("Actual");
            #endregion

            #region Create Default Security Roles
            SecuritySystemRole userRole = ObjectSpace.FindObject<SecuritySystemRole>(new BinaryOperator("Name", "User Role"));
            if (userRole == null)
            {
                userRole = ObjectSpace.CreateObject<SecuritySystemRole>();
                userRole.Name = "User Role";
                userRole.SetTypePermissions<AuditDataItemPersistent>(SecurityOperations.Read, SecuritySystemModifier.Allow);
            }

            //SecuritySystemRole role = ObjectSpace.FindObject<SecuritySystemRole>(new BinaryOperator("Name", "Don't fuck with my shit without permission"));
            //if (role == null)
            //{
            //    role = ObjectSpace.CreateObject<SecuritySystemRole>();
            //    role.Name = "Don't fuck with my shit without permission";
            //    //Prohibit full access to persistent data types (inherited from the XPBaseObject class)
            //    //role.AddPermission(new ObjectAccessPermission(typeof(object), ObjectAccess.AllAccess));
            //    //role.AddPermission(new ObjectAccessPermission(typeof(XPBaseObject),
            //    //   ObjectAccess.AllAccess, ObjectAccessModifier.Deny));
            //    role.SetTypePermissions<XPBaseObject>(SecurityOperations.ReadOnlyAccess, SecuritySystemModifier.Allow);
            //}

            //ObjectSpace.CommitChanges();
            #endregion

            //SecuritySystemUser user = ObjectSpace.FindObject<SecuritySystemUser>(CriteriaOperator.Parse("Oid>?", 0));
            //if (user == null)
            //{
            //    MessageBox.Show("User is null.");

            //}
            //else
            //{
            //    MessageBox.Show(string.Format("User: {0}", user.UserName));
            //}

            CreateDefaultRole();
        }

        private void CreateSchedule(string name)
        {
            Schedule schedule = ObjectSpace.FindObject<Schedule>(CriteriaOperator.Parse("Name=?", name));
            if (schedule == null)
            {
                schedule = ObjectSpace.CreateObject<Schedule>();
                schedule.Name = name;
                ObjectSpace.CommitChanges();
            }
        }
        private void CreateLoopStatus(Int16 sortOrder, string description)
        {
            LoopCheckStatus loopCheckStatus = ObjectSpace.FindObject<LoopCheckStatus>(CriteriaOperator.Parse("Description=?", description));
            if (loopCheckStatus == null)
            {
                loopCheckStatus = ObjectSpace.CreateObject<LoopCheckStatus>();
                loopCheckStatus.Description = description;
                loopCheckStatus.SortOrder = sortOrder;
                ObjectSpace.CommitChanges();
            }
        }
        private void CreateControlSystemType(string name, string description)
        {
            ControlSystemType csType = ObjectSpace.FindObject<ControlSystemType>(CriteriaOperator.Parse("Name=?", name));
            if (csType==null)
            {
                csType = ObjectSpace.CreateObject<ControlSystemType>();
                csType.Name = name;
                csType.Description = description;
                ObjectSpace.CommitChanges();
            }
        }
        private void CreateIOType(string name, string description)
        {
            IOType ioType = ObjectSpace.FindObject<IOType>(CriteriaOperator.Parse("Name=?", name));
            if (ioType == null)
            {
                ioType = ObjectSpace.CreateObject<IOType>();
                ioType.Name = name;
                ioType.Description = description;
                ObjectSpace.CommitChanges();
            }
        }
        private void CreateIOType(string name)
        {
            IOType ioType = ObjectSpace.FindObject<IOType>(CriteriaOperator.Parse("Name=?", name));
            if (ioType == null)
            {
                ioType = ObjectSpace.CreateObject<IOType>();
                ioType.Name = name;
                ObjectSpace.CommitChanges();
            }
        }
        public override void UpdateDatabaseBeforeUpdateSchema() {
            base.UpdateDatabaseBeforeUpdateSchema();
            //if(CurrentDBVersion < new Version("1.1.0.0") && CurrentDBVersion > new Version("0.0.0.0")) {
            //    RenameColumn("DomainObject1Table", "OldColumnName", "NewColumnName");
            //}
        }
        private SecuritySystemRole CreateDefaultRole() {
            
            SecuritySystemRole defaultRole = ObjectSpace.FindObject<SecuritySystemRole>(new BinaryOperator("Name", "Default"));
            if(defaultRole == null) {
                defaultRole = ObjectSpace.CreateObject<SecuritySystemRole>();
                defaultRole.Name = "Default";

                defaultRole.AddObjectAccessPermission<SecuritySystemUser>("[Oid] = CurrentUserId()", SecurityOperations.ReadOnlyAccess);
                defaultRole.AddMemberAccessPermission<SecuritySystemUser>("ChangePasswordOnFirstLogon", SecurityOperations.Write, "[Oid] = CurrentUserId()");
                defaultRole.AddMemberAccessPermission<SecuritySystemUser>("StoredPassword", SecurityOperations.Write, "[Oid] = CurrentUserId()");
                defaultRole.SetTypePermissionsRecursively<SecuritySystemRole>(SecurityOperations.Read, SecuritySystemModifier.Allow);
                defaultRole.SetTypePermissionsRecursively<ModelDifference>(SecurityOperations.ReadWriteAccess, SecuritySystemModifier.Allow);
                defaultRole.SetTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, SecuritySystemModifier.Allow);
            }
            return defaultRole;
        }
    }
}
