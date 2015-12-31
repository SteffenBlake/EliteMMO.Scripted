﻿using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace FormSerialisation
{
    public static class FormSerialisor
    {
        public static void Serialise(Control c, string XmlFileName)
        {
            XmlTextWriter xmlSerialisedForm = new XmlTextWriter(XmlFileName, System.Text.Encoding.Default);
            xmlSerialisedForm.Formatting = Formatting.Indented;
            xmlSerialisedForm.WriteStartDocument();
            xmlSerialisedForm.WriteStartElement("ChildForm");
            AddChildControls(xmlSerialisedForm, c);
            xmlSerialisedForm.WriteEndElement();
            xmlSerialisedForm.WriteEndDocument();
            xmlSerialisedForm.Flush();
            xmlSerialisedForm.Close();
        }
        private static void AddChildControls(XmlTextWriter xmlSerialisedForm, Control c)
        {
            foreach (Control childCtrl in c.Controls)
            {
                if (!(childCtrl is Label) && !(childCtrl is ListView) && !(childCtrl is MenuStrip) &&
                      childCtrl.GetType().ToString() != "System.Windows.Forms.UpDownBase+UpDownEdit" &&
                      childCtrl.GetType().ToString() != "System.Windows.Forms.UpDownBase+UpDownButtons" &&
                      childCtrl.GetType().ToString() != "System.Windows.Forms.Button" && childCtrl.Name != "WaltzPTadd" &&
                      childCtrl.Name != "PartyWaltsList" && childCtrl.Name != "CurePTlist" && childCtrl.Name != "shutdowngroup")
                {
                    xmlSerialisedForm.WriteStartElement("Control");
                    xmlSerialisedForm.WriteAttributeString("Type", childCtrl.GetType().ToString());
                    xmlSerialisedForm.WriteAttributeString("Name", childCtrl.Name);
                    if (childCtrl is TextBox)
                    {
                        xmlSerialisedForm.WriteElementString("Text", ((TextBox)childCtrl).Text);
                    }
                    else if (childCtrl is RadioButton)
                    {
                        xmlSerialisedForm.WriteElementString("Checked", ((RadioButton)childCtrl).Checked.ToString());
                    }
                    else if (childCtrl is NumericUpDown)
                    {
                        xmlSerialisedForm.WriteElementString("Value", ((NumericUpDown)childCtrl).Value.ToString());
                        xmlSerialisedForm.WriteElementString("Enabled", ((NumericUpDown)childCtrl).Enabled.ToString());
                    }
                    else if (childCtrl is GroupBox)
                    {
                        xmlSerialisedForm.WriteElementString("Enabled", ((GroupBox)childCtrl).Enabled.ToString());
                    }
                    else if (childCtrl is CheckedListBox)
                    {
                        CheckedListBox lst = (CheckedListBox)childCtrl;
                        for (int i = 0; i < lst.CheckedIndices.Count; i++)
                        {
                            xmlSerialisedForm.WriteElementString("SelectedIndex" + i.ToString(), (lst.CheckedIndices[i].ToString()));
                        }
                        xmlSerialisedForm.WriteElementString("SelectedIndexcount", (lst.CheckedIndices.Count.ToString()));
                    }
                    else if (childCtrl is ComboBox)
                    {
                        xmlSerialisedForm.WriteElementString("Text", ((ComboBox)childCtrl).Text);
                    }
                    else if (childCtrl is CheckBox)
                    {
                        xmlSerialisedForm.WriteElementString("Checked", ((CheckBox)childCtrl).Checked.ToString());
                        xmlSerialisedForm.WriteElementString("Enabled", ((CheckBox)childCtrl).Enabled.ToString());
                    }
                    if (childCtrl.HasChildren)
                    {
                        if (childCtrl is SplitContainer)
                        {
                            AddChildControls(xmlSerialisedForm, ((SplitContainer)childCtrl).Panel1);
                            AddChildControls(xmlSerialisedForm, ((SplitContainer)childCtrl).Panel2);
                        }
                        else
                        {
                            AddChildControls(xmlSerialisedForm, childCtrl);
                        }
                    }
                    xmlSerialisedForm.WriteEndElement();
                }
            }
        }
        public static void Deserialise(Control c, string XmlFileName)
        {
            if (File.Exists(XmlFileName))
            {
                XmlDocument xmlSerialisedForm = new XmlDocument();
                xmlSerialisedForm.Load(XmlFileName);
                XmlNode topLevel = xmlSerialisedForm.ChildNodes[1];
                foreach (XmlNode n in topLevel.ChildNodes)
                {
                    SetControlProperties((Control)c, n);
                }
            }
        }
        private static void SetControlProperties(Control currentCtrl, XmlNode n)
        {
            string controlName = n.Attributes["Name"].Value;
            if (controlName == "") return;
            string controlType = n.Attributes["Type"].Value;
            Control[] ctrl = currentCtrl.Controls.Find(controlName, true);
            if (ctrl.Length == 0)
            {}
            else
            {
                Control ctrlToSet = GetImmediateChildControl(ctrl, currentCtrl);
                if (ctrlToSet != null)
                {
                    if (ctrlToSet.GetType().ToString() == controlType)
                    {
                        switch (controlType)
                        {
                            case "System.Windows.Forms.CheckedListBox":
                                CheckedListBox ltr = (CheckedListBox)ctrlToSet;
                                var Icount=Convert.ToInt32(n["SelectedIndexcount"].InnerText);
                                foreach (int i in ltr.CheckedIndices)
                                {
                                    ltr.SetItemCheckState(i, CheckState.Unchecked);
                                }
                                for (int i = 0; i < (Icount); i++)
                                {
                                    ltr.SetItemCheckState(Convert.ToInt16(n["SelectedIndex" + i.ToString()].InnerText), CheckState.Checked);
                                    ltr.SetSelected(Convert.ToInt16(n["SelectedIndex" + i.ToString()].InnerText), true);
                                }
                                break;
                            case "System.Windows.Forms.RadioButton":
                                ((RadioButton)ctrlToSet).Checked = Convert.ToBoolean(n["Checked"].InnerText);
                                break;
                            case "System.Windows.Forms.GroupBox":
                                ((GroupBox)ctrlToSet).Enabled = Convert.ToBoolean(n["Enabled"].InnerText);
                                break;
                            case "System.Windows.Forms.NumericUpDown":
                                ((NumericUpDown)ctrlToSet).Value = Convert.ToDecimal(n["Value"].InnerText);
                                ((NumericUpDown)ctrlToSet).Enabled = Convert.ToBoolean(n["Enabled"].InnerText);
                                break;
                            case "System.Windows.Forms.TextBox":
                                ((TextBox)ctrlToSet).Text = n["Text"].InnerText;
                                break;
                            case "System.Windows.Forms.ComboBox":
                                ((ComboBox)ctrlToSet).Text = "";
                                ((ComboBox)ctrlToSet).SelectedText = n["Text"].InnerText;
                                break;
                            case "System.Windows.Forms.CheckBox":
                                ((CheckBox)ctrlToSet).Checked = Convert.ToBoolean(n["Checked"].InnerText);
                                break;
                        }
                        if (n.HasChildNodes && ctrlToSet.HasChildren)
                        {
                            XmlNodeList xnlControls = n.SelectNodes("Control");
                            foreach (XmlNode n2 in xnlControls)
                            {
                                SetControlProperties(ctrlToSet, n2);
                            }
                        }
                    }
                    else
                    {}
                }
                else
                {}
            }
        }
        private static Control GetImmediateChildControl(Control[] ctrl, Control currentCtrl)
        {
            Control c = null;
            for (int i = 0; i < ctrl.Length; i++)
            {
                if ((ctrl[i].Parent.Name == currentCtrl.Name) || (currentCtrl is SplitContainer && ctrl[i].Parent.Parent.Name == currentCtrl.Name))
                {
                    c = ctrl[i];
                    break;
                }
            }
            return c;
        }
    }
}
