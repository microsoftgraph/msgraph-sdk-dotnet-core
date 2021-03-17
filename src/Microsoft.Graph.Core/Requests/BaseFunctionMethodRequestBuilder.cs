// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// The base request builder class for OData functions. This is the base 
    /// class for generated OData function request builders.
    /// </summary>
    public abstract class BaseFunctionMethodRequestBuilder<T> : BaseRequestBuilder where T : IBaseRequest
    {
        private List<string> _parameters = new List<string>();

        /// <summary>
        /// Constructs a new BaseFunctionMethodRequestBuilder.
        /// </summary>
        /// <param name="requestUrl">The URL for the request.</param>
        /// <param name="client">The <see cref="IBaseClient"/> for handling requests.</param>
        public BaseFunctionMethodRequestBuilder(
            string requestUrl,
            IBaseClient client)
            : base(requestUrl, client)
        {
        }

        /// <summary>
        /// Derived classes implement this function to construct the specific request class instance
        /// when a request object is required.
        /// </summary>
        /// <param name="functionUrl">The URL to use for the request.</param>
        /// <param name="options">The query and header options for the request.</param>
        /// <returns>An instance of the request class.</returns>
        protected abstract T CreateRequest(string functionUrl, IEnumerable<Option> options);

        /// <summary>
        /// Builds the request.
        /// </summary>
        /// <param name="options">The query and header options for the request. You can
        /// only use filter and orderby query options.</param>
        /// <returns>The built request.</returns>
        public T Request(IEnumerable<Option> options = null)
        {
            return CreateRequest(this.RequestUrl, options);
        }

        /// <summary>
        /// A helper method for injecting a parameter string for a given name and value
        /// pair. This method handles the nullable case and properly wrapped and escaping
        /// string values.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="nullable">A flag specifying whether the parameter is allowed to be null.</param>
        /// <returns>A string representing the parameter for an OData method call.</returns>
        /// <exception cref="ClientException">Thrown if parameter name is not set.</exception>
        /// <exception cref="ServiceException">Thrown if a non-nullable parameter is passed a null value.</exception>
        protected void SetParameter(string name, object value, bool nullable)
        {
            if (string.IsNullOrEmpty(name))
            { 
                throw new ClientException(
                    new Error
                    {
                        Code = "invalidRequest",
                        Message = "Parameter name must not be null or an empty string."
                    });
            }

            if (value == null && !nullable)
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = "invalidRequest",
                        Message = string.Format("{0} is a required parameter for this method request.", name),
                    });
            }

            string valueAsString = value != null ? value.ToString() : "null";
            if (value is bool)
            {
                valueAsString = valueAsString.ToLower();
            }

            if (value != null && value is string)
            {
                valueAsString = "'" + EscapeStringValue(valueAsString) + "'";
            }

            _parameters.Add(string.Format("{0}={1}", name, valueAsString));
        }

        /// <summary>
        /// Escapes a string value to be safe for OData method calls.
        /// </summary>
        /// <param name="value">The value of the string.</param>
        /// <returns>A properly escaped string.</returns>
        private string EscapeStringValue(string value)
        {
            // Per OData spec, single quotes within a string must be escaped with a second single quote.
            return value.Replace("'", "''");
        }

        /// <summary>
        /// Set the parameter string on the request URL. MUST be used after all 
        /// parameters have been set in the generated request builder. Used for
        /// each section. Adds an empty () if no parameters are present.
        /// </summary>
        protected void SetFunctionParameters()
        { 
            this.RequestUrl = string.Format("{0}({1})", 
                                            this.RequestUrl, 
                                            string.Join(",", _parameters));
        }
    }
}
