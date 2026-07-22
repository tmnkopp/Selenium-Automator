
using System;
using System.Collections.Generic; 
namespace CyberScope.Automator
{ 
    public class ElementValueMap
    {
        public ElementValueMap(){ 
        }    
		public string SRCXPATH { get; set; }
		public string DESTXPATH { get; set; }
        public object[] GetAsRow => new object[] { SRCXPATH, DESTXPATH };
    } 
}
    