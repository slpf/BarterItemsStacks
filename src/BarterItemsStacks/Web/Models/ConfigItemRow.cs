using System.Globalization;

namespace BarterItemsStacks.Web.Models;

public sealed class ConfigItemRow
{
    public string TemplateId { get; }
    public string Name { get; }
    public string Parent { get; }
    public string Category { get; }
    
    public int MaxStackSize { get; set; }
    public int MaxResource { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public double Weight { get; set; }
    public double Price { get; set; }
    
    private string _maxStackSizeText = "";
    private string _maxResourceText = "";
    private string _heightText = "";
    private string _widthText = "";
    private string _weightText = "";
    private string _priceText = "";

    private static string IntToText(int value) => value <= 0 ? "" : value.ToString(CultureInfo.InvariantCulture);
    private static string DoubleToText(double value) => value <= 0 ? "" : value.ToString(CultureInfo.InvariantCulture);
    
    private static bool TryParseAndSet(string value, out int result)
    {
        result = 0;
        
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }
        
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }
    
    private static bool TryParseAndSet(string value, out double result)
    {
        result = 0d;
        
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }
        
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }
    
    public string MaxStackSizeText
    {
        get => _maxStackSizeText;
        set
        {
            _maxStackSizeText = value;
            if (TryParseAndSet(value, out int v))
                MaxStackSize = v;
        }
    }

    public string MaxResourcesText
    {
        get => _maxResourceText;
        set
        {
            _maxResourceText = value;
            if (TryParseAndSet(value, out int v))
                MaxResource = v;
        }
    }

    public string HeightText
    {
        get => _heightText;
        set
        {
            _heightText = value;
            if (TryParseAndSet(value, out int v))
                Height = v;
        }
    }

    public string WidthText
    {
        get => _widthText;
        set
        {
            _widthText = value;
            if (TryParseAndSet(value, out int v))
                Width = v;
        }
    }
    
    public string WeightText
    {
        get => _weightText;
        set
        {
            _weightText = value;
            if (TryParseAndSet(value, out double v))
                Weight = v;
        }
    }

    public string PriceText
    {
        get => _priceText;
        set
        {
            _priceText = value;
            if (TryParseAndSet(value, out double v))
                Price = v;
        }
    }

    public ConfigItemRow(string templateId, string name, string parent, string category, int stackSize, int maxResource, int height, int width, double weight, double price)
    {
        TemplateId = templateId;
        Name = name;
        Parent = parent;
        Category = category;
        MaxStackSize = stackSize;
        MaxResource = maxResource;
        Height = height;
        Width = width;
        Weight = weight;
        Price = price;
        
        _maxStackSizeText = IntToText(stackSize);
        _maxResourceText = IntToText(maxResource);
        _heightText = IntToText(height);
        _widthText = IntToText(width);
        _weightText = DoubleToText(weight);
        _priceText = DoubleToText(price);
        }
}