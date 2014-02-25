using Infragistics.Controls.Grids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace ManagerConsole.UI
{
    public class ColumnWidthPersistence
    {
        public static string GetPersistentColumnsWidthString(XamGrid xamGrid)
        {
            StringBuilder str = new StringBuilder();
            str.Append("<ColumnsWidth Data=\"");
            foreach (var item in xamGrid.Columns.DataColumns)
            {
                str.Append(item.ActualWidth);
                str.Append(",");
            }
            str.Append("\"/>");
            return str.ToString();
        }

        public static void LoadColumnsWidth(XamGrid xamGrid, XElement xml)
        {
            XElement widthElement = xml.Element("ColumnsWidth");
            if (widthElement != null)
            {
                string widthData = widthElement.Attribute("Data").Value;
                string[] allWidth = widthData.Split(',');
                int i = 0;
                foreach (string widthStr in allWidth)
                {
                    if (!string.IsNullOrEmpty(widthStr))
                    {
                        double width = 0;
                        if (double.TryParse(widthStr, out width))
                        {
                            xamGrid.Columns.DataColumns[i].Width = new ColumnWidth(width, false);
                        }
                    }
                    i++;
                }
            }
        }

        public static string GetGridColumnsWidthString(List<XamGrid> xamGrids)
        {

            StringBuilder str = new StringBuilder();
            foreach (XamGrid xamGrid in xamGrids)
            {
                string settingStr = string.Format("<GridSetting Name=\"{0}\" ColumnsWidth=\"", xamGrid.Name);
                str.Append(settingStr);
                foreach (var item in xamGrid.Columns.DataColumns)
                {
                    str.Append(item.ActualWidth);
                    str.Append(",");
                }
                str.Append("\"/>");
            }
            return str.ToString();
        }

        public static void LoadGridColumnsWidth(ObservableCollection<XamGrid> xamGrids, XElement xml)
        {
            foreach (XElement widthElement in xml.Elements("GridSetting"))
            {
                if (widthElement != null)
                {
                    string widthData = widthElement.Attribute("ColumnsWidth").Value;
                    string[] allWidth = widthData.Split(',');
                    string gridName = widthElement.Attribute("Name").Value;
                    int i = 0;
                    foreach (string widthStr in allWidth)
                    {
                        if (!string.IsNullOrEmpty(widthStr))
                        {
                            double width = 0;
                            if (double.TryParse(widthStr, out width))
                            {
                                xamGrids.SingleOrDefault(x => x.Name == gridName).Columns.DataColumns[i].Width = new ColumnWidth(width, false);
                            }
                        }
                        i++;
                    }
                }
            }
        }
    }
}
