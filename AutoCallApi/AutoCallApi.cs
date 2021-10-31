using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace AutoCallApi
{
    public partial class AutoCallApi : Form
    {
        private static INode _Config;
        private static List<KeyValuePair<string, string>> _Header;
        private static List<TestCase> _TC;
        public AutoCallApi()
        {
            InitializeComponent();
            cbType.SelectedIndex = 0;
        }
        public void InsertMessage(string message)
        {
            string msg = txtLog.Text;
            msg += "[" + DateTime.Now.ToString("HH:mm:ss - dd/MM/yyyy") + "] - " + message;
            msg += Environment.NewLine;
            txtLog.Text = msg;
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }
        private void btnCreateConfig_Click(object sender, EventArgs e)
        {
            using (Process p = new Process())
            {
                p.StartInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    FileName = System.Environment.CurrentDirectory + "\\CreateConfig.html"
                };
                p.Start();
                InsertMessage("Mở Tạo file file cấu hình");
            }
        }
        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "json files (*.json)|*.json";
            tvHeader.Nodes.Clear();
            tvBody.Nodes.Clear();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                InsertMessage("Nhập file: " + dlg.FileName);
                string msg = GetConfigFromFile(dlg.FileName);
                if (string.IsNullOrEmpty(msg))
                {
                    //Cho phép chạy test
                    InsertMessage("File cấu hình hợp lệ!");
                    btnRun.Enabled = true;
                }
                else
                {
                    InsertMessage("File cấu hình không hợp lệ: " + msg);
                    btnRun.Enabled = false;
                }
            }
            else
            {
                btnRun.Enabled = false;
            }
        }
        private void btnRun_Click(object sender, EventArgs e)
        {
            InsertMessage("Bắt đầu tao request...");
            _TC = null;
            if (!Uri.IsWellFormedUriString(txtUrl.Text, UriKind.Absolute)) 
            {
                InsertMessage("Url không hợp lệ");
                MessageBox.Show("Url không hợp lệ");
                return;
            }
            txtUrl.Enabled = false;
            btnRun.Enabled = false;
            btnSave.Enabled = false;
            btnImport.Enabled = false;
            string msg = CreateTC();
            if (!string.IsNullOrEmpty(msg))
            {
                txtUrl.Enabled = false;
                btnRun.Enabled = true;
                InsertMessage("Xảy ra lỗi khi sinh dữ liệu call api: " + msg);
                return;
            }
            InsertMessage($"Đã sinh ra {_TC.Count} request. Bắt đầu chạy request...");
            for (int i = 0; i < _TC.Count(); i++)
            {
                var tc = _TC[i];
                CallApi(ref tc);
                _TC[i] = tc;
            }
            txtUrl.Enabled = true;
            btnRun.Enabled = true;
            btnImport.Enabled = true;
            btnSave.Enabled = true;
            var a = tvBody.SelectedNode;
            InsertMessage("Đã chạy xong tất cả request.");
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                ExcelPackage exPkg = new ExcelPackage();
                ExcelWorksheet exWS = exPkg.Workbook.Worksheets.Add("[" + cbType.Text + "]" + txtUrl);
                //Header
                using (var rng = exWS.Cells[1, 1, 2, 1])
                {
                    rng.Merge = true;
                    rng.Value = "STT";
                    rng.Style.Font.Color.SetColor(Color.FromName("white"));
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(47, 84, 150));
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }
                using (var rng = exWS.Cells[1, 2, 2, 2])
                {
                    rng.Merge = true;
                    rng.Value = "Tên";
                    rng.Style.Font.Color.SetColor(Color.FromName("white"));
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(47, 84, 150));
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }
                using (var rng = exWS.Cells[1, 3, 2, 3])
                {
                    rng.Merge = true;
                    rng.Value = "Body";
                    rng.Style.Font.Color.SetColor(Color.FromName("white"));
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(47, 84, 150));
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }
                using (var rng = exWS.Cells[1, 4, 1, 5])
                {
                    rng.Merge = true;
                    rng.Value = "Response";
                    rng.Style.Font.Color.SetColor(Color.FromName("white"));
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(47, 84, 150));
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }
                exWS.Cells[2, 4].Value = "Code"; 
                exWS.Cells[2, 4].Style.Font.Color.SetColor(Color.FromName("white"));
                exWS.Cells[2, 4].Style.Font.Bold = true;
                exWS.Cells[2, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                exWS.Cells[2, 4].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(47, 84, 150));
                exWS.Cells[2, 4].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                exWS.Cells[2, 4].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                exWS.Cells[2, 4].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                exWS.Cells[2, 4].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                exWS.Cells[2, 5].Value = "Content";
                exWS.Cells[2, 5].Style.Font.Color.SetColor(Color.FromName("white"));
                exWS.Cells[2, 5].Style.Font.Bold = true;
                exWS.Cells[2, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                exWS.Cells[2, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(47, 84, 150));
                exWS.Cells[2, 5].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                exWS.Cells[2, 5].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                exWS.Cells[2, 5].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                exWS.Cells[2, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                //
                //Data
                for (int i = 0; i < _TC.Count(); i++)
                {
                    var tc = _TC[i];
                    exWS.Cells[i + 3, 1].Value = (i + 1).ToString();
                    exWS.Cells[i + 3, 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 2].Value = tc.Name;
                    exWS.Cells[i + 3, 2].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 2].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 3].Value = tc.Body;
                    exWS.Cells[i + 3, 3].Style.WrapText = true;
                    exWS.Cells[i + 3, 3].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 3].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 3].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 4].Value = tc.Code.ToString();
                    exWS.Cells[i + 3, 4].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 4].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 4].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 4].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 5].Value = tc.Response;
                    exWS.Cells[i + 3, 5].Style.WrapText = true;
                    exWS.Cells[i + 3, 5].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 5].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 5].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    exWS.Cells[i + 3, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                string path = Environment.CurrentDirectory + "\\Result\\[" + cbType.Text + "]" + txtUrl.Text.Replace("https://", "").Replace("http://", "") + ".xlsx";
                exWS.Protection.IsProtected = false;
                exWS.Protection.AllowSelectLockedCells = false;
                exPkg.SaveAs(new System.IO.FileInfo(path));
                InsertMessage("Đã lưu kết quả tại: " + path);
            }
            catch (Exception ex)
            {
                InsertMessage("Lỗi khi lưu kết quả: " + ex.Message);
            }
        }
        private string CreateTC()
        {
            try
            {
                var tcs = new List<TestCase>();
                tcs.Add(new TestCase
                {
                    Name = "Tất cả các trường giá trị nhập đúng.",
                    Body = _Config.GetTrueContent()
                });
                tcs.AddRange(_Config.GetFalseContent().Select(x => new TestCase
                {
                    Name = x.Key,
                    Body = x.Value
                }).ToList());
                _TC = tcs;
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        private string GetConfigFromFile(string fileName)
        {
            try
            {
                _Config = null;
                string content = System.IO.File.ReadAllText(fileName);
                ConfigData data = JsonConvert.DeserializeObject<ConfigData>(content);
                _Header = data.Header;
                var listChildren = new List<INode>();
                foreach (var item in data.Body)
                {
                    listChildren.Add(NodeProvider.NewNode(item, 1));
                }
                _Config = new NodeObject
                {
                    Level = 0,
                    Fields = listChildren
                };
                foreach (var item in _Header)
                {
                    tvHeader.Nodes.Add(new TreeNode
                    {
                        Text = item.Key,
                        ToolTipText = item.Value
                    });
                }
                foreach (var item in ((NodeObject)_Config).Fields)
                {
                    TreeNode node = new TreeNode
                    {
                        Text = item.Title
                    };
                    if (item.Type == 1)
                    {
                        ShowTreeBody(ref node, ((NodeObject)item).Fields);
                    }
                    else
                    {
                        node.ToolTipText = $"True Value: + {((NodeValue)item).TrueValue}{(((NodeValue)item).Values.Count == 0 ? "" : "\nFalseValue:\n")}{String.Join("\n+ ", ((NodeValue)item).Values)}";
                    }
                    tvBody.Nodes.Add(node);
                    item.Node = node;
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public void CallApi(ref TestCase tc)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage
                    {
                        RequestUri = new Uri(txtUrl.Text),
                        Method = GetMethod()
                    };
                    foreach (var item in _Header)
                    {
                        request.Headers.Add(item.Key, item.Value);
                    }
                    request.Content = new StringContent(tc.Body, Encoding.UTF8, "application/json");
                    var response = client.SendAsync(request).Result;
                    tc.Code = (int)response.StatusCode;
                    tc.Response = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception ex)
            {
                tc.Code = -1;
                tc.Response = ex.Message;
            }
        }
        public void ShowTreeBody(ref TreeNode parent, List<INode> children)
        {
            foreach (var child in children)
            {
                var node = new TreeNode
                {
                    Text = child.Title
                };
                if (child.Type == 1)
                {
                    ShowTreeBody(ref node, ((NodeObject)child).Fields);
                }
                else
                {
                    node.ToolTipText = $"True Value: {((NodeValue)child).TrueValue}{(((NodeValue)child).Values.Count == 0 ? "" : "\nFalseValue:")}{String.Join("", ((NodeValue)child).Values.Select(x => $"\n+ [{x.Key}] {x.Value}").ToList())}";
                }
                parent.Nodes.Add(node);
                child.Node = node;
            }
        }
        private HttpMethod GetMethod()
        {
            HttpMethod method = HttpMethod.Get;
            switch (cbType.Text)
            {
                case "GET":
                    method = HttpMethod.Get;
                    break;
                case "POST":
                    method = HttpMethod.Post;
                    break;
                case "PUT":
                    method = HttpMethod.Put;
                    break;
                case "PATCH":
                    method = HttpMethod.Patch;
                    break;
                case "DELETE":
                    method = HttpMethod.Delete;
                    break;
                case "HEAD":
                    method = HttpMethod.Head;
                    break;
                case "OPTIONS":
                    method = HttpMethod.Options;
                    break;
                default:
                    break;
            }
            return method;
        }
    }
    public interface INode
    {
        public string Title { get; set; }
        public int Type { get; set;  }
        public string GetTrueContent();
        public List<KeyValuePair<string, string>> GetFalseContent();
        public TreeNode Node { get; set; }
        public bool Checked { get { return Node.Checked; } }
    }
    public class NodeObject : INode
    {
        public string Title { get; set; }
        public int Level { get; set; }
        public List<INode> Fields { get; set; }
        public int Type { get; set; } = 1;
        public TreeNode Node { get; set; }
        public string GetTrueContent()
        {
            string prefix = "";
            for (int i = 0; i < Level; i++) prefix += "\t";
            string content = (Level == 0 ? "" : $"\"{Title}\": ") + "{";
            List<string> childrenContent = new List<string>();
            foreach (var field in Fields)
            {
                childrenContent.Add("\n" + prefix + "\t" + field.GetTrueContent());
            }
            content += String.Join(",", childrenContent);
            content += "\n" + prefix + "}";

            return content;
        }
        public List<KeyValuePair<string, string>> GetFalseContent()
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            string prefix = "";
            for (int i = 0; i < Level; i++) prefix += "\t";
            for (int i = 0; i < Fields.Count; i++)
            {
                var tmp = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("", prefix + "{") };
                for (int j = 0; j < Fields.Count; j++)
                {
                    if (i != j)
                    {
                        string content = Fields[j].GetTrueContent();
                        tmp = tmp.Select(x => new KeyValuePair<string, string>(x.Key, x.Value + "\n" + prefix + "\t" + content + (j < Fields.Count - 1 ? "," : ""))).ToList();
                    }
                    else
                    {
                        tmp = (from r in tmp
                               join c in Fields[j].GetFalseContent() on 1 equals 1
                               select new KeyValuePair<string, string>(c.Key, r.Value == "" ? "" : r.Value + "\n" + prefix + "\t" + c.Value + (j < Fields.Count - 1 ? "," : ""))).Where(x => x.Value != "").ToList();
                    }
                }
                tmp = tmp.Select(x => new KeyValuePair<string, string>(x.Key, x.Value + "\n" + "}")).ToList();
                result.AddRange(tmp);
            }
            return result;
        }
        public NodeObject()
        {
            Fields = new List<INode>();
        }
        public bool HasCheckedChild { get { return Fields.Where(x => x.Checked || x.Type == 1 && ((NodeObject)x).HasCheckedChild).Count() > 0; } }
    }
    public class NodeValue : INode
    {
        public string Title { get; set; }
        public int Level { get; set; }
        public int Type { get; set; } = 2;
        public string TrueValue { get; set; }
        public List<KeyValuePair<string, string>> Values { get; set; }
        public TreeNode Node { get; set; }
        public List<KeyValuePair<string, string>> GetFalseContent()
        {
            return Values.Select(x => new KeyValuePair<string, string>(x.Key, $"\"{Title}\": {x.Value}")).ToList();
        }

        public string GetTrueContent()
        {
            return(TrueValue == "" ? "" : $"\"{Title}\": {TrueValue}");
        }
    }
    public class NodeProvider
    {
        public static INode NewNode(dynamic obj, int level)
        {
            if (Convert.ToInt32(obj[0]) == 1)
            {
                var values = new List<KeyValuePair<string, string>>();
                foreach (var item in obj[3])
                {
                    values.Add(new KeyValuePair<string, string>(Convert.ToString(item[0]), Convert.ToString(item[1])));
                }
                return new NodeValue
                {
                    Title = Convert.ToString(obj[1]),
                    Level = level,
                    TrueValue = Convert.ToString(obj[2]),
                    Values = values
                };
            }
            else
            {
                var children = new List<INode>();
                foreach (var item in obj[2])
                {
                    children.Add(NewNode(item, level + 1));
                }
                return new NodeObject
                {
                    Title = Convert.ToString(obj[1]),
                    Level = level,
                    Fields = children
                };
            }
        }
    }
    public class TestCase
    {
        public string Name { get; set; }
        public string Body { get; set; }
        public int Code { get; set; }
        public string Response { get; set; }
    }
    public class ConfigData
    {
        public List<KeyValuePair<string, string>> Header;
        public List<dynamic> Body;
    }
}
