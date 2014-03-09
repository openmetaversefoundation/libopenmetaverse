﻿using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;

using Prebuild.Core.Attributes;
using Prebuild.Core.Interfaces;
using Prebuild.Core.Nodes;
using Prebuild.Core.Utilities;
using System.CodeDom.Compiler;

namespace Prebuild.Core.Targets
{

    /// <summary>
    /// 
    /// </summary>
    [Target("vs2012")]
    public class VS2012Target : VSGenericTarget
    {
        #region Fields
        string solutionVersion = "12.00";
        string productVersion = "11.0.51106.01";
        string schemaVersion = "2.0";
        string versionName = "Visual Studio 2012";
        string name = "vs2012";
        VSVersion version = VSVersion.VS11;

        Hashtable tools;
        Kernel kernel;

        /// <summary>
        /// Gets or sets the solution version.
        /// </summary>
        /// <value>The solution version.</value>
        public override string SolutionVersion
        {
            get
            {
                return solutionVersion;
            }
        }
        /// <summary>
        /// Gets or sets the product version.
        /// </summary>
        /// <value>The product version.</value>
        public override string ProductVersion
        {
            get
            {
                return productVersion;
            }
        }
        /// <summary>
        /// Gets or sets the schema version.
        /// </summary>
        /// <value>The schema version.</value>
        public override string SchemaVersion
        {
            get
            {
                return schemaVersion;
            }
        }
        /// <summary>
        /// Gets or sets the name of the version.
        /// </summary>
        /// <value>The name of the version.</value>
        public override string VersionName
        {
            get
            {
                return versionName;
            }
        }
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public override VSVersion Version
        {
            get
            {
                return version;
            }
        }
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get
            {
                return name;
            }
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VS2012Target"/> class.
        /// </summary>
        public VS2012Target()
            : base()
        {
        }

        #endregion
    }
}
