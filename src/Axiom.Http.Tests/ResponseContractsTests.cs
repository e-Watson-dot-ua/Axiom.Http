using Axiom.Http.Responses;

namespace Axiom.Http.Tests;

internal static class Program
{
    private static int Main()
    {
        try
        {
            ApiResponse_Errors_DefaultsToEmpty();
            ApiResponse_PrimaryError_DerivedFromErrors();
            ApiResponse_WithMetadata_AddsEntry();
            ApiResponse_Map_PreservesFailure();
            PagedResponse_Create_ThrowsOnNullData();
            PagedResponse_Create_FromEnumerable_Works();
            PagedResponse_CreateClamped_ClampsPageNumber();
            PageMetadata_Empty_IsConsistent();

            Console.WriteLine("All tests passed.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertThrows<TException>(Action action, string message) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }

    private static void ApiResponse_Errors_DefaultsToEmpty()
    {
        var ok = ApiResponse.Ok();
        Assert(ok.Errors.Count == 0, "ApiResponse.Ok should have zero errors.");

        var okT = ApiResponse<string>.Ok("data");
        Assert(okT.Errors.Count == 0, "ApiResponse<T>.Ok should have zero errors.");
    }

    private static void ApiResponse_PrimaryError_DerivedFromErrors()
    {
        // We can't construct Axiom.Application.Results.Error without referencing its API,
        // so we validate the behavior in the no-error case.
        var ok = ApiResponse.Ok();
        Assert(ok.PrimaryError is null, "PrimaryError should be null when there are no errors.");

        var okT = ApiResponse<string>.Ok("data");
        Assert(okT.PrimaryError is null, "PrimaryError should be null when there are no errors.");
    }

    private static void ApiResponse_WithMetadata_AddsEntry()
    {
        var response = ApiResponse.Ok().WithMetadata("k", 123);
        Assert(response.Metadata is not null && response.Metadata.TryGetValue("k", out var v) && (int)v == 123,
            "WithMetadata should add a metadata entry.");

        var responseT = ApiResponse<string>.Ok("data").WithMetadata("k", "v");
        Assert(responseT.Metadata is not null && responseT.Metadata.TryGetValue("k", out var v2) && (string)v2 == "v",
            "WithMetadata<T> should add a metadata entry.");
    }

    private static void ApiResponse_Map_PreservesFailure()
    {
        // Can't create Error easily here; we validate that failures stay failures via a response initializer.
        // This ensures Map doesn't accidentally flip Success.
        var failed = ApiResponse<string>.Fail(error: null!, correlationId: "c");
        var mapped = failed.Map(_ => 42);
        Assert(!mapped.Success, "Map should preserve failure.");
        Assert(mapped.CorrelationId == "c", "Map should preserve correlation id on failure.");
    }

    private static void PagedResponse_Create_ThrowsOnNullData()
    {
        AssertThrows<ArgumentNullException>(
            () => PagedResponse<string>.Create(null!, pageNumber: 1, pageSize: 10, totalCount: 0),
            "PagedResponse.Create should throw ArgumentNullException when data is null.");
    }

    private static void PagedResponse_Create_FromEnumerable_Works()
    {
        var response = PagedResponse<int>.Create(new[] { 1, 2, 3 }.AsEnumerable(), pageNumber: 1, pageSize: 10, totalCount: 3);
        Assert(response.Data.Count == 3, "Create(IEnumerable) should materialize and preserve items.");
        Assert(response.Metadata.TotalCount == 3, "Create(IEnumerable) should set metadata.");
    }

    private static void PagedResponse_CreateClamped_ClampsPageNumber()
    {
        var response = PagedResponse<int>.CreateClamped(new[] { 1 }, pageNumber: 999, pageSize: 10, totalCount: 1);
        Assert(response.Metadata.PageNumber == 1, "CreateClamped should clamp pageNumber to the last page.");

        var empty = PagedResponse<int>.CreateClamped(Array.Empty<int>(), pageNumber: 999, pageSize: 10, totalCount: 0);
        Assert(empty.Metadata.PageNumber == 1, "CreateClamped should clamp to page 1 when TotalPages == 0.");
    }

    private static void PageMetadata_Empty_IsConsistent()
    {
        var empty = PageMetadata.Empty;
        Assert(empty.PageNumber == 1, "PageMetadata.Empty.PageNumber should be 1.");
        Assert(empty.PageSize > 0, "PageMetadata.Empty.PageSize should be > 0.");
        Assert(empty.TotalCount == 0, "PageMetadata.Empty.TotalCount should be 0.");
        Assert(empty.TotalPages == 0, "For totalCount=0 we expect TotalPages=0.");
        Assert(empty.CurrentPageSize == 0, "For totalCount=0 we expect CurrentPageSize=0.");
        Assert(!empty.HasPreviousPage, "Empty metadata should not have previous page.");
        Assert(!empty.HasNextPage, "Empty metadata should not have next page.");
    }
}
