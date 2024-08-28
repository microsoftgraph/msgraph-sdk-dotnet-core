using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Xunit;

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests.Content;

public class BatchRequestContentStepsTests
{
    private readonly BatchRequestContentSteps _steps;

    public BatchRequestContentStepsTests()
    {
        _steps = new BatchRequestContentSteps();
        _steps.Add("1", new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me")));
        _steps["2"] = new BatchRequestStep("2", new HttpRequestMessage(HttpMethod.Post, "https://graph.microsoft.com/v1.0/me"));
        _steps["uuid-123"] = new BatchRequestStep("uuid-123", new HttpRequestMessage(HttpMethod.Patch, "https://graph.microsoft.com/v1.0/me"));
    }

    [Fact]
    public void BatchRequestContentSteps_Indexer_Get_ReturnsValue()
    {
        Assert.Equal("1", _steps["1"].RequestId);
    }

    [Fact]
    public void BatchRequestContentSteps_Keys_ReturnsKeysInInsertionOrder()
    {
        var keys = _steps.Keys.ToList();
        Assert.True(keys.Count == 3);
        Assert.Equal("1", keys[0]);
        Assert.Equal("2", keys[1]);
        Assert.Equal("uuid-123", keys[2]);
    }

    [Fact]
    public void BatchRequestContentSteps_Values_ReturnsValuesInInsertionOrder()
    {
        var values = _steps.Values.ToList();
        Assert.True(values.Count == 3);
        Assert.Equal("1", values[0].RequestId);
        Assert.Equal("2", values[1].RequestId);
        Assert.Equal("uuid-123", values[2].RequestId);
    }

    [Fact]
    public void BatchRequestContentSteps_Count_ReturnsCount()
    {
        Assert.Equal(3, _steps.Count);
    }

    [Fact]
    public void BatchRequestContentSteps_IsReadOnly_ReturnsTrue()
    {
        Assert.True(_steps.IsReadOnly);
    }

    [Fact]
    public void BatchRequestContentSteps_Add_ThrowsExceptionOnDuplicateValue()
    {
        Assert.Throws<ArgumentException>(() => _steps.Add("1", new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me"))));
    }

    [Fact]
    public void BatchRequestContentSteps_Add_ThrowsExceptionOnNullKey()
    {
        Assert.Throws<ArgumentNullException>(() => _steps.Add(null, new BatchRequestStep("3", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me"))));
    }

    [Fact]
    public void BatchRequestContentSteps_Add_ThrowsExceptionOnEmptyKey()
    {
        Assert.Throws<ArgumentNullException>(() => _steps.Add(string.Empty, new BatchRequestStep("3", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me"))));
    }

    [Fact]
    public void BatchRequestContentSteps_Contains_ReturnsFalse()
    {
        Assert.DoesNotContain(new KeyValuePair<string, BatchRequestStep>("4", new BatchRequestStep("4", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me"))), _steps);
        Assert.DoesNotContain(new KeyValuePair<string, BatchRequestStep>("1", new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Post, "https://graph.microsoft.com/v1.0/me"))), _steps);
    }

    [Fact]
    public void BatchRequestContentSteps_ContainsKey_ReturnsTrue()
    {
        Assert.True(_steps.ContainsKey("1"));
    }

    [Fact]
    public void BatchRequestContentSteps_ContainsKey_ReturnsFalse()
    {
        Assert.False(_steps.ContainsKey("4"));
    }

    [Fact]
    public void BatchRequestContentSteps_Remove_ReturnsTrue()
    {
        var steps = new BatchRequestContentSteps();
        steps.Add("1", new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me")));
        Assert.True(steps.Remove("1"));
        Assert.False(steps.ContainsKey("1"));
    }

    [Fact]
    public void BatchRequestContentSteps_Remove_ReturnsFalse()
    {
        var steps = new BatchRequestContentSteps();
        steps.Add("1", new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me")));
        Assert.False(steps.Remove("2"));
        Assert.True(steps.ContainsKey("1"));
    }

    [Fact]
    public void BatchRequestContentSteps_TryGetValue_ReturnsTrue()
    {
        var steps = new BatchRequestContentSteps();
        steps.Add("1", new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me")));
        Assert.True(steps.TryGetValue("1", out var value));
        Assert.Equal("1", value.RequestId);
    }

    [Fact]
    public void BatchRequestContentSteps_TryGetValue_ReturnsFalse()
    {
        var steps = new BatchRequestContentSteps();
        steps.Add("1", new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me")));
        Assert.False(steps.TryGetValue("2", out var value));
        Assert.Null(value);
    }

    [Fact]
    public void BatchRequestContentSteps_Clear_RemovesAllItems()
    {
        var steps = new BatchRequestContentSteps();
        steps.Add("1", new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me")));
        steps.Add("2", new BatchRequestStep("2", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me")));
        steps.Clear();
        Assert.True(steps.Count == 0);
        Assert.True(steps.Keys.Count == 0);
        Assert.True(steps.Values.Count == 0);
    }

    [Fact]
    public void BatchRequestContentSteps_CopyTo_CopiesItems()
    {
        var steps = new BatchRequestContentSteps();
        steps.Add("1", new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me")));
        steps.Add("2", new BatchRequestStep("2", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me")));
        var array = new KeyValuePair<string, BatchRequestStep>[2];
        steps.CopyTo(array, 0);
        Assert.Equal("1", array[0].Key);
        Assert.Equal("2", array[1].Key);
    }

    [Fact]
    public void BatchRequestContentSteps_CopyTo_ThrowsExceptionOnNullArray()
    {
        var steps = new BatchRequestContentSteps();
        Assert.Throws<ArgumentNullException>(() => steps.CopyTo(null, 0));
    }

    [Fact]
    public void BatchRequestContentSteps_CopyTo_ThrowsExceptionOnNegativeArrayIndex()
    {
        var steps = new BatchRequestContentSteps();
        Assert.Throws<ArgumentOutOfRangeException>(() => steps.CopyTo(new KeyValuePair<string, BatchRequestStep>[2], -1));
    }

    [Fact]
    public void BatchRequestContentSteps_CopyTo_ThrowsExceptionOnInsufficientArraySpace()
    {
        var steps = new BatchRequestContentSteps();
        steps.Add("1", new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me")));
        Assert.Throws<ArgumentException>(() => steps.CopyTo(new KeyValuePair<string, BatchRequestStep>[1], 1));
    }

    [Fact]
    public void BatchRequestContentSteps_Enumerator_ReturnsItems()
    {
        var steps = new BatchRequestContentSteps();
        steps.Add("1", new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me")));
        steps.Add("2", new BatchRequestStep("2", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me")));
        foreach (var item in steps)
        {
            Assert.True(item.Key == "1" || item.Key == "2");
        }
    }

    [Fact]
    public void BatchRequestContentSteps_Enumerator_ReturnsItemsInInsertionOrder()
    {
        var steps = new BatchRequestContentSteps();
        steps.Add("1", new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me")));
        steps.Add("2", new BatchRequestStep("2", new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me")));
        var enumerator = steps.GetEnumerator();
        enumerator.MoveNext();
        Assert.Equal("1", enumerator.Current.Key);
        enumerator.MoveNext();
        Assert.Equal("2", enumerator.Current.Key);
    }
}
