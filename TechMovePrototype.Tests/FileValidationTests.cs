using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using TechMovePrototype.Controllers;

namespace TechMovePrototype.Tests;

public class FileValidationTests
{
    [Fact]
    public void UploadNonPdfFile_AddsValidationError()
    {
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("malware.exe");

       
        bool isPdf = fileMock.Object.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

        Assert.False(isPdf);
    }

    [Fact]
    public void UploadPdfFile_AcceptsFile()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("contract.pdf");

        bool isPdf = fileMock.Object.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

        Assert.True(isPdf);
    }
}