using Bonsai;
using Bonsai.Shaders;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

[Combinator]
[WorkflowElementCategory(ElementCategory.Transform)]
[Description("Returns the texture resource with the specified name for each notification in the sequence.")]
public class GrabTextureByName
{
    /// <summary>
    /// Gets or sets the name of the texture resource to retrieve.
    /// </summary>
    [TypeConverter(typeof(TextureNameConverter))]
    [Description("The name of the texture resource to retrieve.")]
    public string TextureName { get; set; }

    static Texture LoadTexture(ShaderWindow window, string textureName)
    {
        if (string.IsNullOrEmpty(textureName))
        {
            throw new InvalidOperationException("A texture name must be specified.");
        }

        return window.ResourceManager.Load<Texture>(textureName);
    }

    /// <summary>
    /// Returns the texture resource named by the <see cref="TextureName"/> property
    /// for each notification in an observable sequence. The property is read on every
    /// notification, so the texture can be switched at run time.
    /// </summary>
    public IObservable<Texture> Process<TSource>(IObservable<TSource> source)
    {
        return ShaderManager.WindowSource.Select(window =>
        {
            var cachedName = default(string);
            var cachedTexture = default(Texture);
            return source.Select(input =>
            {
                var textureName = TextureName;
                if (cachedTexture == null || cachedName != textureName)
                {
                    cachedTexture = LoadTexture(window, textureName);
                    cachedName = textureName;
                }

                return cachedTexture;
            });
        }).Switch();
    }

    /// <summary>
    /// Returns the texture resource matching each name in an observable sequence of
    /// texture names.
    /// </summary>
    public IObservable<Texture> Process(IObservable<string> source)
    {
        return ShaderManager.WindowSource.Select(window =>
        {
            var cachedName = default(string);
            var cachedTexture = default(Texture);
            return source.Select(textureName =>
            {
                if (cachedTexture == null || cachedName != textureName)
                {
                    cachedTexture = LoadTexture(window, textureName);
                    cachedName = textureName;
                }

                return cachedTexture;
            });
        }).Switch();
    }
}
