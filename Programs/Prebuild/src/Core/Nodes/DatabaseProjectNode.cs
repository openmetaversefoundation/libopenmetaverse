using System;
using System.Collections;
using System.Text;
using System.Xml;

using Prebuild.Core.Attributes;
using Prebuild.Core.Interfaces;
using Prebuild.Core.Utilities;

namespace Prebuild.Core.Nodes
{
    [DataNode("DatabaseProject")]
    public class DatabaseProjectNode : DataNode
    {
        string name;
        string path;
        string fullpath;
        Guid guid = Guid.NewGuid();
        ArrayList authors = new ArrayList();
        ArrayList references = new ArrayList();

        public Guid Guid
        {
            get { return this.guid; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public string Path
        {
            get { return this.path; }
        }

        public string FullPath
        {
            get { return this.fullpath; }
        }

        public IEnumerable References
        {
            get { return this.references; }
        }

        public override void Parse(System.Xml.XmlNode node)
        {
            this.name = Helper.AttributeValue(node, "name", this.name);
            this.path = Helper.AttributeValue(node, "path", this.name);

            try
            {
                this.fullpath = Helper.ResolvePath(this.path);
            }
            catch
            {
                throw new WarningException("Could not resolve Solution path: {0}", this.path);
            }

            Kernel.Instance.CurrentWorkingDirectory.Push();

            try
            {
                Helper.SetCurrentDir(this.fullpath);

                if (node == null)
                {
                    throw new ArgumentNullException("node");
                }

                foreach (XmlNode child in node.ChildNodes)
                {
                    IDataNode dataNode = Kernel.Instance.ParseNode(child, this);

                    if (dataNode == null)
                        continue;

                    if (dataNode is AuthorNode)
                        this.authors.Add(dataNode);
                    else if (dataNode is DatabaseReferenceNode)
                        this.references.Add(dataNode);
                }
            }
            finally
            {
                Kernel.Instance.CurrentWorkingDirectory.Pop();
            }

            base.Parse(node);
        }
    }
}
