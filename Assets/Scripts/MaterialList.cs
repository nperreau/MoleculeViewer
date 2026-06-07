using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct AtomMaterial
{
    [FormerlySerializedAs("atom")]
    public string symbol;
    public Material material;
}

[CreateAssetMenu(fileName = "New Materials List", menuName = "Materials List")]
public class MaterialList : ScriptableObject
{
    [SerializeField] private List<AtomMaterial> materials = new();
    [SerializeField] private Material defaultMaterial;

    private Dictionary<string, Material> materialMap;
    private bool cacheGenerated = false;

    private Material Validate(Material material) => material && material.enableInstancing ? material: throw new Exception($"Material {material.name} is has {nameof(material.enableInstancing)} set to false.");

    public Material GetMaterial(string symbol)
    {
        if (!cacheGenerated)
            CacheMaterials();

        if (!string.IsNullOrWhiteSpace(symbol) && materialMap.TryGetValue(symbol.Trim(), out Material material) && material)
            return Validate(material);

        return Validate(defaultMaterial);
    }

    public void InvalidateCache() => cacheGenerated = false;

    private void CacheMaterials()
    {
        materialMap ??= new Dictionary<string, Material>();
        materialMap.Clear();

        if (materials == null)
        {
            cacheGenerated = true;
            return;
        }

        foreach (AtomMaterial atomMaterial in materials)
        {
            if (string.IsNullOrWhiteSpace(atomMaterial.symbol) || !atomMaterial.material)
                continue;

            materialMap[atomMaterial.symbol.Trim()] = Validate(atomMaterial.material);
        }

        cacheGenerated = true;
    }

    private void Awake() => InvalidateCache();

    private void OnValidate() => InvalidateCache();
}