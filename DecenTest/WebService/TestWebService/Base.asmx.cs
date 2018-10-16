using System;
using System.Web.Services;
using System.Diagnostics;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using System.ComponentModel;

namespace TestWebService
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name = "BasicHttpBinding_Base", Namespace = "http://www.srrc.org.cn")]
    public partial class Base : System.Web.Services.Protocols.SoapHttpClientProtocol
    {

        private MonitorHeader monitorHeaderValueField;

        private ProviderResponse providerResponseValueField;

        private System.Threading.SendOrPostCallback B_FScanOperationCompleted;

        /// <remarks/>
        public Base()
        {
            this.Url = "http://192.168.120.77:17095/110000011190066/ESMB/B_FScan";
        }

        public MonitorHeader MonitorHeaderValue
        {
            get
            {
                return this.monitorHeaderValueField;
            }
            set
            {
                this.monitorHeaderValueField = value;
            }
        }

        public ProviderResponse ProviderResponseValue
        {
            get
            {
                return this.providerResponseValueField;
            }
            set
            {
                this.providerResponseValueField = value;
            }
        }

        /// <remarks/>
        public event B_FScanCompletedEventHandler B_FScanCompleted;

        /// <remarks/>
        [WebMethod]
        [System.Web.Services.Protocols.SoapHeaderAttribute("ProviderResponseValue", Direction = System.Web.Services.Protocols.SoapHeaderDirection.Out)]
        [System.Web.Services.Protocols.SoapHeaderAttribute("MonitorHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("B_FScan",
            RequestNamespace = "http://www.srrc.org.cn",
            ResponseNamespace = "http://www.srrc.org.cn",
            Use = System.Web.Services.Description.SoapBindingUse.Literal,
            ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public string B_FScan([System.Xml.Serialization.XmlElementAttribute(IsNullable = true)] string input)
        {
            object[] results = this.Invoke("B_FScan", new object[] {
                        input});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult BeginB_FScan(string input, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("B_FScan", new object[] {
                        input}, callback, asyncState);
        }

        /// <remarks/>
        public string EndB_FScan(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void B_FScanAsync(string input)
        {
            this.B_FScanAsync(input, null);
        }

        /// <remarks/>
        public void B_FScanAsync(string input, object userState)
        {
            if ((this.B_FScanOperationCompleted == null))
            {
                this.B_FScanOperationCompleted = new System.Threading.SendOrPostCallback(this.OnB_FScanOperationCompleted);
            }
            this.InvokeAsync("B_FScan", new object[] {
                        input}, this.B_FScanOperationCompleted, userState);
        }

        private void OnB_FScanOperationCompleted(object arg)
        {
            if ((this.B_FScanCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.B_FScanCompleted(this, new B_FScanCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        public new void CancelAsync(object userState)
        {
            base.CancelAsync(userState);
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.srrc.org.cn")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.srrc.org.cn", IsNullable = true)]
    public partial class ProviderResponse : System.Web.Services.Protocols.SoapHeader
    {

        private string bizResCdField;

        private string bizResTextField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public string bizResCd
        {
            get
            {
                return this.bizResCdField;
            }
            set
            {
                this.bizResCdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public string bizResText
        {
            get
            {
                return this.bizResTextField;
            }
            set
            {
                this.bizResTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.srrc.org.cn")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.srrc.org.cn", IsNullable = true)]
    public partial class MonitorHeader : System.Web.Services.Protocols.SoapHeader
    {

        private string bSCodeField;

        private string bizKeyField;

        private string pSCodeField;

        private string transIdField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public string BSCode
        {
            get
            {
                return this.bSCodeField;
            }
            set
            {
                this.bSCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public string BizKey
        {
            get
            {
                return this.bizKeyField;
            }
            set
            {
                this.bizKeyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public string PSCode
        {
            get
            {
                return this.pSCodeField;
            }
            set
            {
                this.pSCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public string TransId
        {
            get
            {
                return this.transIdField;
            }
            set
            {
                this.transIdField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.6.1055.0")]
    public delegate void B_FScanCompletedEventHandler(object sender, B_FScanCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class B_FScanCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal B_FScanCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
                base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public string Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }

}
