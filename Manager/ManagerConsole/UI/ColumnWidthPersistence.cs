using Infragistics.Controls.Grids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

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
            string widthData = xml.Attribute("Data").Value;
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

        public static string GetGridColumnsWidthString(XamGrid xamGrid)
        {
            StringBuilder str = new StringBuilder();
            string settingStr = string.Format("<GridSetting Name=\"{0}\" ColumnsWidth=\"", xamGrid.Name);
            str.Append(settingStr);
            foreach (var item in xamGrid.Columns.DataColumns)
            {
                str.Append(item.ActualWidth);
                str.Append(",");
            }
            str.Append("\"/>");
            return str.ToString();
        }

        public static void LoadGridColumnsWidth(XamGrid xamGrid, XElement xml)
        {
            string widthData = xml.Attribute("ColumnsWidth").Value;
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
}
