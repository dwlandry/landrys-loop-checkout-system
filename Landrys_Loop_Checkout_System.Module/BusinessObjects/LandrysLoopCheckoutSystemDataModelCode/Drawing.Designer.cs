﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
namespace Landrys_Loop_Checkout_System.Module.BusinessObjects.Db151516LoopCheckout
{

    public partial class Drawing : XPObject
    {
        string fNumber;
        public string Number
        {
            get { return fNumber; }
            set { SetPropertyValue<string>("Number", ref fNumber, value); }
        }
        string fDescription;
        public string Description
        {
            get { return fDescription; }
            set { SetPropertyValue<string>("Description", ref fDescription, value); }
        }
    }

}
