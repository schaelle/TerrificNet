﻿using System.Collections.Generic;

namespace TerrificNet.Thtml.Rendering
{
	public interface IIncrementalDomRenderer
	{
		void ElementVoid(string tagName, string key, Dictionary<string, string> staticPropertyValuePairs, Dictionary<string, string> propertyValuePairs);
		void ElementOpen(string tagName, string key, Dictionary<string, string> staticPropertyValuePairs, Dictionary<string, string> propertyValuePairs);
		void ElementOpenStart(string tagName, string key, Dictionary<string, string> staticPropertyValuePairs, Dictionary<string, string> propertyValuePairs);
		void Attr(string name, string value);
		void Text(string content);
		void ElementOpenEnd();
		void ElementClose(string tagName);
	}
}