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

    public partial class Instrument : XPObject
    {
        string fTagNumber;
        public string TagNumber
        {
            get { return fTagNumber; }
            set { SetPropertyValue<string>("TagNumber", ref fTagNumber, value); }
        }
        PID fPID;
        [Association(@"InstrumentReferencesPID")]
        public PID PID
        {
            get { return fPID; }
            set { SetPropertyValue<PID>("PID", ref fPID, value); }
        }
        JunctionBox fJunctionBox;
        [Association(@"InstrumentReferencesJunctionBox")]
        public JunctionBox JunctionBox
        {
            get { return fJunctionBox; }
            set { SetPropertyValue<JunctionBox>("JunctionBox", ref fJunctionBox, value); }
        }
        LocationPlan fLocationPlan;
        [Association(@"InstrumentReferencesLocationPlan")]
        public LocationPlan LocationPlan
        {
            get { return fLocationPlan; }
            set { SetPropertyValue<LocationPlan>("LocationPlan", ref fLocationPlan, value); }
        }
        string fCalibration;
        public string Calibration
        {
            get { return fCalibration; }
            set { SetPropertyValue<string>("Calibration", ref fCalibration, value); }
        }
        IOType fIOType;
        [Association(@"InstrumentReferencesIOType")]
        public IOType IOType
        {
            get { return fIOType; }
            set { SetPropertyValue<IOType>("IOType", ref fIOType, value); }
        }
        Loop fLoop;
        [Association(@"InstrumentReferencesLoop")]
        public Loop Loop
        {
            get { return fLoop; }
            set { SetPropertyValue<Loop>("Loop", ref fLoop, value); }
        }
        LoopDrawing fLoopDrawing;
        [Association(@"InstrumentReferencesLoopDrawing")]
        public LoopDrawing LoopDrawing
        {
            get { return fLoopDrawing; }
            set { SetPropertyValue<LoopDrawing>("LoopDrawing", ref fLoopDrawing, value); }
        }
        ControlSystem fControlSystem;
        [Association(@"InstrumentReferencesControlSystem")]
        public ControlSystem ControlSystem
        {
            get { return fControlSystem; }
            set { SetPropertyValue<ControlSystem>("ControlSystem", ref fControlSystem, value); }
        }
        InstrumentType fInstrumentType;
        [Association(@"InstrumentReferencesInstrumentType")]
        public InstrumentType InstrumentType
        {
            get { return fInstrumentType; }
            set { SetPropertyValue<InstrumentType>("InstrumentType", ref fInstrumentType, value); }
        }
        string fServiceDescription;
        public string ServiceDescription
        {
            get { return fServiceDescription; }
            set { SetPropertyValue<string>("ServiceDescription", ref fServiceDescription, value); }
        }
        Company fResponsibleCompany;
        public Company ResponsibleCompany
        {
            get { return fResponsibleCompany; }
            set { SetPropertyValue<Company>("ResponsibleCompany", ref fResponsibleCompany, value); }
        }
    }

}
