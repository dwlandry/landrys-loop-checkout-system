using System;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout;
using Landrys_Loop_Checkout_System.Module.BusinessObjects;

namespace Landrys_Loop_Checkout_System.Module.DatabaseUpdate {
    public class Updater : ModuleUpdater {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
            base(objectSpace, currentDBVersion) {
        }
        public override void UpdateDatabaseAfterUpdateSchema() {
            base.UpdateDatabaseAfterUpdateSchema();

            CreateLoopStatus(1, "Not Ready for Check");
            CreateLoopStatus(2, "Ready for Check");
            CreateLoopStatus(3, "Being Checked");
            CreateLoopStatus(4, "Engineering Problems");
            CreateLoopStatus(5, "Contractor Wiring Mods");
            CreateLoopStatus(6, "Complete but waiting on graphics repair");
            CreateLoopStatus(7, "Complete but waiting on config repair");
            CreateLoopStatus(8, "Complete but waiting on signature");
            CreateLoopStatus(9, "Complete - Ready for Startup");

            JobInfo jobInfo = ObjectSpace.FindObject<JobInfo>(CriteriaOperator.Parse("Oid>0"));
            if (jobInfo == null)
            {
                jobInfo = ObjectSpace.CreateObject<JobInfo>();
                jobInfo.Number = "Enter the Job Number";
                jobInfo.Description = "Enter the Job Description";
                jobInfo.ClientName = "Enter the Client Name";
                ObjectSpace.CommitChanges();
            }

            CreateControlSystemType("DCS", "Primary digital control system");
            CreateControlSystemType("SIS", "Safety systems, triconex, etc.");
            CreateControlSystemType("Other", "Local PLC systems; manufactuer supplied PLCs");

            CreateIOType("AI");
            CreateIOType("AO");
            CreateIOType("DI");
            CreateIOType("DO");

            CreateSchedule("Original");
            CreateSchedule("Current");
            CreateSchedule("Actual");

            CreateAdminRole();
            CreateDefaultRole();
            EnsureUsersHaveAdministratorRole();
            ObjectSpace.CommitChanges();
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
        }
        private PermissionPolicyRole CreateAdminRole() {
            PermissionPolicyRole adminRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Administrators");
            if (adminRole == null) {
                adminRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                adminRole.Name = "Administrators";
                adminRole.IsAdministrative = true;
            }
            return adminRole;
        }
        private PermissionPolicyRole CreateDefaultRole() {
            PermissionPolicyRole defaultRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Default");
            if(defaultRole == null) {
                defaultRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                defaultRole.Name = "Default";
            }
            defaultRole.IsAdministrative = true;
            return defaultRole;
        }

        private void EnsureUsersHaveAdministratorRole()
        {
            PermissionPolicyRole adminRole = CreateAdminRole();
            foreach (PermissionPolicyUser user in ObjectSpace.GetObjects<PermissionPolicyUser>())
            {
                bool hasAdmin = false;
                foreach (PermissionPolicyRole role in user.Roles)
                {
                    if (role == adminRole)
                    {
                        hasAdmin = true;
                        break;
                    }
                }
                if (!hasAdmin)
                {
                    user.Roles.Add(adminRole);
                }
            }
        }
    }
}
