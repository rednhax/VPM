using System;
using VPM.Services;

namespace VPM.Tests
{
    /// <summary>
    /// Quick test to verify PreviewImageValidator works correctly with various paths
    /// </summary>
    public class PreviewImageValidatorTests
    {
        public static void RunTests()
        {
            Console.WriteLine("=== PreviewImageValidator Tests ===\n");

            // Test case 1: The problematic path from the issue
            TestPath("saves/scene/_look/_testitou/preview.jpg", true, "Creator folder with underscores");
            
            // Test case 2: Standard creator folder
            TestPath("saves/scene/testitou/preview.jpg", true, "Standard creator folder");
            
            // Test case 3: Nested creator folder structure
            TestPath("saves/scene/testitou/_look_/image.jpg", true, "Nested creator structure");
            
            // Test case 4: Direct image in saves/scene
            TestPath("saves/scene/preview.jpg", true, "Direct image in saves/scene");
            
            // Test case 5: Texture should be excluded
            TestPath("saves/scene/testitou/textures/skin.jpg", false, "Texture path (should be excluded)");
            
            // Test case 6: Plugin preset
            TestPath("custom/pluginpresets/mypreset/preview.jpg", true, "Plugin preset");
            
            // Test case 7: Appearance preset
            TestPath("custom/atom/person/appearance/myappearance/preview.jpg", true, "Appearance preset");
            
            // Test case 8: Clothing
            TestPath("custom/clothing/myoutfit/preview.jpg", true, "Clothing");
            
            // Test case 9: Hair
            TestPath("custom/hair/myhair/preview.jpg", true, "Hair");
            
            // Test case 10: Addon packages should be excluded
            TestPath("addonpackages/something/image.jpg", false, "Addon package (should be excluded)");
            
            // Test case 11: Custom scripts should be excluded
            TestPath("custom/scripts/something/image.jpg", false, "Custom script (should be excluded)");
            
            // Test case 12: Custom sounds should be excluded
            TestPath("custom/sounds/something/audio.jpg", false, "Custom sound (should be excluded)");
            
            // Test case 13: Filename with preview keyword
            TestPath("custom/assets/myasset/preview_image.jpg", true, "Preview keyword in filename");
            
            // Test case 14: Filename with thumb keyword
            TestPath("custom/assets/myasset/thumbnail.jpg", true, "Thumbnail keyword");
            
            // Test case 15: Filename with icon keyword
            TestPath("custom/assets/myasset/icon.jpg", true, "Icon keyword");
            
            // Test case 16: Preset_ prefix
            TestPath("custom/atom/person/skin/preset_myskin.jpg", true, "Preset_ prefix in skin");
            
            // Test case 17: Subscene
            TestPath("custom/subscene/myscene/preview.jpg", true, "Subscene");
            
            // Test case 18: Pose
            TestPath("saves/person/pose/mypose/preview.jpg", true, "Pose");
            
            // Test case 19: Appearance save
            TestPath("saves/person/appearance/myappearance/preview.jpg", true, "Appearance save");
            
            // Test case 20: Full save
            TestPath("saves/person/full/mysave/preview.jpg", true, "Full save");

            Console.WriteLine("\n=== All tests completed ===");
        }

        private static void TestPath(string path, bool expectedResult, string description)
        {
            var result = PreviewImageValidator.IsPreviewImage(path);
            var status = result == expectedResult ? "✓ PASS" : "✗ FAIL";
            Console.WriteLine($"{status}: {description}");
            Console.WriteLine($"  Path: {path}");
            Console.WriteLine($"  Expected: {expectedResult}, Got: {result}\n");
        }
    }
}
