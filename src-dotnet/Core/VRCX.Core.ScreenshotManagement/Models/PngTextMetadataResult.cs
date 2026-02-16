namespace VRCX.Core.ScreenshotManagement.Models;

/// <param name="XmpMetadata">Extensible Metadata Platform (XMP, or ISO 16684-1), Use by VRChat</param>
/// <param name="VrcxMetadata">VRCX storage a JSON in Description PNG Text Chunk</param>
/// <param name="LfsLikeMetadata">I don't know what the heck is "LFS image metadata", use by old game mods</param>
public record PngTextMetadataResult(
    string? XmpMetadata,
    string? VrcxMetadata,
    string? LfsLikeMetadata
);