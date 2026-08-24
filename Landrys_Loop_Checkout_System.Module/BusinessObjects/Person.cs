using System;
using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace Landrys_Loop_Checkout_System.Module.BusinessObjects
{
    /// <summary>
    /// Stand-in for DevExpress.Persistent.BaseImpl.Person, which was removed from XAF in v25.2.
    /// </summary>
    [DefaultProperty(nameof(FullName))]
    public class Person : BaseObject
    {
        private string _firstName;
        private string _lastName;
        private string _middleName;
        private string _email;
        private DateTime _birthday;

        public Person(Session session) : base(session) { }

        public string FirstName
        {
            get { return _firstName; }
            set { SetPropertyValue(nameof(FirstName), ref _firstName, value); }
        }

        public string LastName
        {
            get { return _lastName; }
            set { SetPropertyValue(nameof(LastName), ref _lastName, value); }
        }

        public string MiddleName
        {
            get { return _middleName; }
            set { SetPropertyValue(nameof(MiddleName), ref _middleName, value); }
        }

        public string Email
        {
            get { return _email; }
            set { SetPropertyValue(nameof(Email), ref _email, value); }
        }

        public DateTime Birthday
        {
            get { return _birthday; }
            set { SetPropertyValue(nameof(Birthday), ref _birthday, value); }
        }

        [PersistentAlias("Concat(IsNull([FirstName], ''), ' ', IsNull([LastName], ''))")]
        public string FullName
        {
            get { return $"{FirstName} {LastName}".Trim(); }
        }
    }
}
