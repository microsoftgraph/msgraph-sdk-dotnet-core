using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Graph;

/// <summary>
/// Extension helpers for the <see cref="IParseNode"/>
/// </summary>
public static class ParseNodeExtensions
{
    internal static string GetErrorMessage(this IParseNode responseParseNode)
    {
        var errorParseNode = responseParseNode.GetChildNode("error");
        // concatenate the error code and message
        return $"{errorParseNode?.GetChildNode("code")?.GetStringValue()} : {errorParseNode?.GetChildNode("message")?.GetStringValue()}";
    }
}
