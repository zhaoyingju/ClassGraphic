using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ClassTool
{
    public static class Extension
    {
        public static Array ToNewCollection(this TreeNodeCollection nodes)
        {
           

            Array arr = Array.CreateInstance(typeof(TreeNode), nodes.Count);

            nodes.CopyTo(arr, 0);

            //nodes.CopyTo()
            //TreeNodeCollection newNodes = new TreeNodeCollection();
            //foreach (TreeNode node in nodes)
            //{
            //    newNodes.Add(node);
            //}
            return arr;
        }
    }
}
