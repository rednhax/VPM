using System.Collections.Generic;

namespace VPM.Models
{
    /// <summary>
    /// Defines known tags for clothing and hair items in VaM packages.
    /// Tags are extracted from .vam files inside VAR packages.
    /// Based on VaM's built-in tag system.
    /// </summary>
    public static class ContentTagDefinitions
    {
        // ============================================
        // HAIR TAGS
        // ============================================
        
        /// <summary>
        /// Hair region tags - where the hair is located on the body
        /// </summary>
        public static readonly List<string> HairRegionTags = new()
        {
            "head", "face", "genital", "torso", "arms", "legs", "full body"
        };

        /// <summary>
        /// Hair type tags - style characteristics
        /// </summary>
        public static readonly List<string> HairTypeTags = new()
        {
            "short", "long"
        };

        /// <summary>
        /// Hair other tags - miscellaneous
        /// </summary>
        public static readonly List<string> HairOtherTags = new()
        {
            "tail", "beard", "mustache", "eyebrows", "eyelashes"
        };

        /// <summary>
        /// All known hair tags combined
        /// </summary>
        public static readonly HashSet<string> AllHairTags = new(System.StringComparer.OrdinalIgnoreCase)
        {
            // Region
            "head", "face", "genital", "torso", "arms", "legs", "full body",
            // Type
            "short", "long",
            // Other
            "tail", "beard", "mustache", "eyebrows", "eyelashes"
        };

        // ============================================
        // CLOTHING TAGS
        // ============================================

        /// <summary>
        /// Clothing region tags - where the clothing is worn
        /// </summary>
        public static readonly List<string> ClothingRegionTags = new()
        {
            "head", "torso", "hip", "arms", "hands", "legs", "feet", "neck", "full body"
        };

        /// <summary>
        /// Clothing type tags - type of garment
        /// </summary>
        public static readonly List<string> ClothingTypeTags = new()
        {
            "bra", "panties", "underwear", "shorts", "pants", "top", "shirt", "skirt", "dress",
            "shoes", "socks", "stockings", "gloves", "jewelry", "accessory", "hat", "mask",
            "bodysuit", "bottom", "glasses", "sweater"
        };

        /// <summary>
        /// Clothing other tags - style and miscellaneous
        /// </summary>
        public static readonly List<string> ClothingOtherTags = new()
        {
            "back", "costume", "fantasy", "heels", "jeans", "lingerie", "sneakers", "swimwear",
            "bikini", "casual", "formal", "sexy", "sport", "uniform", "vintage", "modern"
        };

        /// <summary>
        /// All known clothing tags combined
        /// </summary>
        public static readonly HashSet<string> AllClothingTags = new(System.StringComparer.OrdinalIgnoreCase)
        {
            // Region
            "head", "torso", "hip", "arms", "hands", "legs", "feet", "neck", "full body",
            // Type
            "bra", "panties", "underwear", "shorts", "pants", "top", "shirt", "skirt", "dress",
            "shoes", "socks", "stockings", "gloves", "jewelry", "accessory", "hat", "mask",
            "bodysuit", "bottom", "glasses", "sweater",
            // Other
            "back", "costume", "fantasy", "heels", "jeans", "lingerie", "sneakers", "swimwear",
            "bikini", "casual", "formal", "sexy", "sport", "uniform", "vintage", "modern"
        };

        /// <summary>
        /// Gets all unique tags across both clothing and hair
        /// </summary>
        public static HashSet<string> GetAllUniqueTags()
        {
            var all = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var tag in AllClothingTags) all.Add(tag);
            foreach (var tag in AllHairTags) all.Add(tag);
            return all;
        }

        /// <summary>
        /// Categorizes a tag into its type (Region, Type, or Other)
        /// </summary>
        public static string GetTagCategory(string tag, bool isClothing)
        {
            if (string.IsNullOrEmpty(tag)) return "Other";
            
            var lowerTag = tag.ToLowerInvariant();
            
            if (isClothing)
            {
                if (ClothingRegionTags.Contains(lowerTag)) return "Region";
                if (ClothingTypeTags.Contains(lowerTag)) return "Type";
                return "Other";
            }
            else
            {
                if (HairRegionTags.Contains(lowerTag)) return "Region";
                if (HairTypeTags.Contains(lowerTag)) return "Type";
                return "Other";
            }
        }
    }
}
