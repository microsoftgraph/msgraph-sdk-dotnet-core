namespace Microsoft.Graph
{
    using System;
    using System.Linq;

    public static class TypeExtensions
    {
        /// <summary>
        /// Determine whether the value is a simple OData type. We may already have special deserialization instructions
        /// for some types (see *Converters). 
        /// http://docs.oasis-open.org/odata/odata-csdl-xml/v4.01/odata-csdl-xml-v4.01.html#_Toc38530338
        /// https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/blob/dev/src/GraphODataTemplateWriter/CodeHelpers/CSharp/TypeHelperCSharp.cs
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSimpleOdataValue(this Type type)
        {
            return
                new Type[] {
                    typeof(string),
                    typeof(bool),
                    typeof(double),
                    typeof(decimal),
                    typeof(long),
                    typeof(int),
                    typeof(Guid)
                }.Contains(type);
        }
    }
}