﻿/**************************************************************************
*                           MIT License
* 
* Copyright (C) 2015 Frederic Chaxel <fchaxel@free.fr>
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be included
* in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*
*********************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.BACnet;
using System.IO.BACnet.Serialize;
using System.Diagnostics;

namespace Yabe
{
    public partial class AlarmSummary : Form
    {
        BacnetClient comm; BacnetAddress adr;
        IList<BacnetGetEventInformationData> Alarms;

        public AlarmSummary(ImageList img_List, BacnetClient comm, BacnetAddress adr, uint device_id)
        {
            InitializeComponent();
            this.Text = "Active Alarms on Device Id " + device_id.ToString();
            this.comm = comm;
            this.adr = adr;            

            bool MoreEvent;
            TAlarmList.ImageList = img_List;

            // get the Alarm summary
            // Addentum 135-2012av-1 : Deprecate Execution of GetAlarmSummary, GetEVentInformation instead
            // -> parameter 2 in the method call
             if (comm.GetAlarmSummaryOrEventRequest(adr, Properties.Settings.Default.AlarmByGetEventInformation, out Alarms, out MoreEvent) == true)
            {
                LblInfo.Visible = false;
                AckText.Enabled = AckBt.Enabled = true;

                FillTreeNode();
            }
            if (MoreEvent == true)
                PartialLabel.Visible = true;
        }
        private static string GetEventStateNiceName(String name)
        {
            name = name.Substring(12);
            name = name.Replace('_', ' ');
            name = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
            return name;
        }
        private static string GetEventEnableNiceName(String name)
        {
            name = name.Substring(13);
            name = name.Replace('_', ' ');
            name = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
            return name;
        }

        private void FillTreeNode()
        {
            TAlarmList.BeginUpdate();

            TAlarmList.Nodes.Clear();

            // fill the Treenode
            foreach (BacnetGetEventInformationData alarm in Alarms)
            {
                IList<BacnetValue> name;
                comm.ReadPropertyRequest(adr, alarm.objectIdentifier, BacnetPropertyIds.PROP_OBJECT_NAME, out name);

                int icon = MainDialog.GetIconNum(alarm.objectIdentifier.type);
                TreeNode tn = new TreeNode(name[0].Value.ToString(), icon, icon);
                tn.ToolTipText = alarm.objectIdentifier.ToString();
                tn.Tag = alarm;
                TAlarmList.Nodes.Add(tn);

                icon = Int32.MaxValue; // out bound
                tn.Nodes.Add(new TreeNode("Alarm state : " + GetEventStateNiceName(alarm.eventState.ToString()), icon, icon));

                bool SomeTodo = false;

                TreeNode tn2 = new TreeNode("Ack Required :", icon, icon);               
                for (int i = 0; i < 3; i++)
                {
                    if (alarm.acknowledgedTransitions.ToString()[i] == '0')
                    {
                        BacnetEventNotificationData.BacnetEventEnable bee = (BacnetEventNotificationData.BacnetEventEnable)(1 << i);
                        String text = GetEventEnableNiceName(bee.ToString()) + " since " + alarm.eventTimeStamps[i].Time.ToString();
                        tn2.Nodes.Add(new TreeNode(text, icon, icon));
                        SomeTodo = true;
                    }
                }

                if (SomeTodo == false) tn2 = new TreeNode("No Ack Required, all is OK", icon, icon); //tn2.Nodes.Add(new TreeNode("Nothing to do, all is OK", icon, icon));
                tn.Nodes.Add(tn2);

                TAlarmList.EndUpdate();

                TAlarmList.ExpandAll();
            }

            if (Alarms.Count == 0)
            {
                LblInfo.Visible = true;
                LblInfo.Text = "Empty event list ... all is OK";
            }
        }

        private void AckBt_Click(object sender, EventArgs e)
        {            
            TreeNode tn = TAlarmList.SelectedNode;
            if (tn == null) return;
            while (tn.Parent!=null) tn=tn.Parent;  // go up
   
            BacnetGetEventInformationData alarm = (BacnetGetEventInformationData)tn.Tag; // the alam content

            bool SomeChanges = false;
            for (int i = 0; i < 3; i++)
            {
                if (alarm.acknowledgedTransitions.ToString()[i] == '0') // Transition to be ack
                {
                    BacnetGenericTime bgt;

                    if (alarm.eventTimeStamps != null)
                        bgt = alarm.eventTimeStamps[i];
                    else // Deprecate Execution of GetAlarmSummary
                    {
                        // Read the event time stamp
                        IList<BacnetValue> values;
                        if (comm.ReadPropertyRequest(adr, alarm.objectIdentifier, BacnetPropertyIds.PROP_EVENT_TIME_STAMPS, out values, 0, (uint)i) == false)
                        {
                            Trace.TraceWarning("Error reading PROP_EVENT_TIME_STAMPS");
                            return;
                        }
                        String s1 = ((BacnetValue[])(values[0].Value))[0].ToString(); // Date & 00:00:00 for Hour
                        String s2 = ((BacnetValue[])(values[0].Value))[1].ToString(); // 00:00:00 & Time
                        DateTime dt = Convert.ToDateTime(s1.Split(' ')[0] + " " + s2.Split(' ')[1]);
                        bgt = new BacnetGenericTime(dt, BacnetTimestampTags.TIME_STAMP_DATETIME);
                    }

                    // something to clarify : BacnetEventStates & BacnetEventEnable !!!
                    BacnetEventNotificationData.BacnetEventStates eventstate = (BacnetEventNotificationData.BacnetEventStates)(2 - i);

                    if (comm.AlarmAcknowledgement(adr, alarm.objectIdentifier, eventstate, AckText.Text, bgt,
                                new BacnetGenericTime(DateTime.Now, BacnetTimestampTags.TIME_STAMP_DATETIME)) == true)
                    {
                        alarm.acknowledgedTransitions.SetBit((byte)i, true);
                        SomeChanges = true;
                    }
                }

                if (SomeChanges)
                    FillTreeNode();
            }
        }

        // No more used
        private void TAlarmList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode tn=e.Node;
            while (tn.Parent != null) tn = tn.Parent;

            if (tn.ToolTipText == "")
            {
                BacnetGetEventInformationData alarm = (BacnetGetEventInformationData)tn.Tag;
                IList<BacnetValue> name;

                comm.ReadPropertyRequest(adr, alarm.objectIdentifier, BacnetPropertyIds.PROP_OBJECT_NAME, out name);

                tn.ToolTipText = tn.Text;
                tn.Text = name[0].Value.ToString();

            }

        }

    }
}