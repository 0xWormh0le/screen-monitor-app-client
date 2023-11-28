using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Client.Model
{
    public class ContentClickEventArgs : EventArgs
    {
        private readonly object _contentObject = null;

        public ContentClickEventArgs(object obj)
        {
            this._contentObject = obj;
        }

        public object ContentObject
        {
            get { return _contentObject; }
        }
    }

    public delegate void ContentClickEventHandler(object sender, ContentClickEventArgs args);    
}
