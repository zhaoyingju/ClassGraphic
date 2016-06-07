using ClassRelation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace ClassTool
{
    public partial class MainForm : Form
    {
        private string _chcheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CacheDll");

        private Dictionary<string, ClassRelation> _assembleFiles;
        public MainForm()
        {
            InitializeComponent();
            _assembleFiles = new Dictionary<string, ClassRelation>();
            LoadInitialAssemblies();
        }
        void LoadInitialAssemblies()
        {
            System.Reflection.Assembly[] initialAssemblies = {
                typeof(object).Assembly,
                typeof(System.IO.File).Assembly,
                typeof(Uri).Assembly,
                typeof(System.Linq.Enumerable).Assembly,
                typeof(System.Xml.XmlDocument).Assembly,
                typeof(System.Drawing.Bitmap).Assembly,
                typeof(System.Net.Cookie).Assembly,
            };
            foreach (System.Reflection.Assembly asm in initialAssemblies)
            {
                var assembleName = asm.GetName();
                string key = string.Format("{0}({1})", assembleName.Name, assembleName.Version);
                ClassRelation classR = new ClassRelation();
                classR.LoadFromAssemblyFile(asm.Location);
                _assembleFiles[key] = classR;
            }
            LoadCacheAssembly();
            BindTreeView();
        }

        private void BindTreeView()
        {
            AssembleTree.LabelEdit = false;//不可编辑  
            AssembleTree.Nodes.Clear();
            foreach (var item in _assembleFiles)
            {
                TreeNode assembleNode = new TreeNode();
                assembleNode.Text = item.Key;
                AssembleTree.Nodes.Add(assembleNode);
                foreach (var namespaceItem in item.Value.NamespaceContainer)
                {
                    TreeNode namespaceNode = new TreeNode();
                    namespaceNode.Text = namespaceItem.Key;
                    assembleNode.Nodes.Add(namespaceNode);
                    foreach (var typeItem in namespaceItem.Value)
                    {
                        TreeNode typeNode = new TreeNode();
                        typeNode.Text = typeItem.Value.Name;
                        typeNode.Tag = new Tuple<ClassRelation, TypeInfo>(item.Value, typeItem.Value);
                        namespaceNode.Nodes.Add(typeNode);
                    }
                }
            }
            //_assembleFiles.Clear();
        }

        private void ShowClassGriphic(TreeNode clickNode)
        {
            if (clickNode == null)
                return;
            Tuple<ClassRelation, TypeInfo> nodeData = clickNode.Tag as Tuple<ClassRelation, TypeInfo>;
            if (nodeData != null && nodeData.Item2 is ClassInfo)
            {
                var img = nodeData.Item1.Draw(nodeData.Item2 as ClassInfo);
                this.pictureBox1.Image = img;
                this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
                // this.pictureBox1.Width = this.pictureBox1.Image.Width;
                // this.pictureBox1.Height = this.pictureBox1.Image.Height;
            }
        }

        private bool IsMatch(TreeNode node, string text)
        {

            return false;
        }

        private void FilterTypeInfo(string search)
        {
            //if (string.IsNullOrWhiteSpace(search))
            //{

            //    return;
            //}
            BindTreeView();

            foreach (TreeNode moduleNode in AssembleTree.Nodes.ToNewCollection())
            {
                if (moduleNode.Text.ToLower().Contains(search.ToLower()))
                    continue;

                foreach (TreeNode namespaceNode in moduleNode.Nodes.ToNewCollection())
                {
                    if (moduleNode.Text.ToLower().Contains(search.ToLower()))
                        continue;
                    foreach (TreeNode typeNode in namespaceNode.Nodes.ToNewCollection())
                    {
                        if (!typeNode.Text.ToLower().Contains(search.ToLower()))
                        {
                            typeNode.Remove();
                        }
                    }
                    if (namespaceNode.Nodes.Count == 0)
                        namespaceNode.Remove();
                }
                if (moduleNode.Nodes.Count == 0)
                    moduleNode.Remove();
            }
        }

        /// <summary>
        /// 移除叶子节点
        /// </summary>
        /// <returns></returns>
        private void FilterTypeInfo(TreeNode tn, string search)
        {
            for (int i = tn.Nodes.Count - 1; i >= 0; i--)
            {
                FilterTypeInfo(tn.Nodes[i], search);
            }
            Tuple<ClassRelation, TypeInfo> typeNode = tn.Tag as Tuple<ClassRelation, TypeInfo>;
            if (typeNode != null && tn.Nodes.Count == 0)
            {
                var fullname = typeNode.Item2.FullName;
                bool flag = fullname.ToLower().Contains(search.ToLower());
                if (flag)
                    tn.BackColor = Color.HotPink;
            }
        }

        private void AssembleTree_DragDrop(object sender, DragEventArgs e)
        {
            var paths = ((System.Array)e.Data.GetData(DataFormats.FileDrop)) as string[];
            foreach (var path in paths)
            {
                string assembleName = Path.GetFileNameWithoutExtension(path);
                ClassRelation relation = new ClassRelation();
                relation.LoadFromAssemblyFile(path);
                _assembleFiles[assembleName] = relation;
                try
                {
                    File.Copy(path, Path.Combine(_chcheDir, Path.GetFileName(path)), true);
                }
                catch { }

            }
            BindTreeView();
        }

        private void AssembleTree_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            var key = this.textBox1.Text;
            FilterTypeInfo(key);
        }

        private void AssembleTree_Click(object sender, EventArgs e)
        {
            var clickNode = AssembleTree.SelectedNode;
            ShowClassGriphic(clickNode);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (AssembleTree.SelectedNode != null)
            {
                ShowClassGriphic(AssembleTree.SelectedNode);
            }
        }

        private void LoadCacheAssembly()
        {
            if (!Directory.Exists(_chcheDir))
                Directory.CreateDirectory(_chcheDir);

            foreach (var path in "*.dll;*.exe".Split(';').SelectMany(g => Directory.EnumerateFiles(_chcheDir, g)))
            {
                string assembleName = Path.GetFileNameWithoutExtension(path);
                ClassRelation relation = new ClassRelation();
                relation.LoadFromAssemblyFile(path);
                _assembleFiles[assembleName] = relation;

            }
        }
    }
}
