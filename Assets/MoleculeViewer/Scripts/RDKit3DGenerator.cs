using System;
using System.Collections.Generic;
using GraphMolWrap;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class RDKit3DGenerator : MonoBehaviour
{
    private const int MaxInstancesPerDraw = 1023;

    [Header("Chemical Input")]
    [SerializeField] private string smiles = "";

    [Header("Rendering Configurations")]
    [SerializeField]
    private Mesh atomMesh;
    [SerializeField, Min(0.01f)]
    private float coordinateScale = 1f;
    [SerializeField]
    private bool centerMolecule = true;
    [SerializeField]
    private Transform moleculeRoot;

    private Matrix4x4 RootMatrix => moleculeRoot ? moleculeRoot.localToWorldMatrix : Matrix4x4.identity;

    [SerializeField]
    private Transform invalidSMILES;
    
    [SerializeField]
    private float atomScale = 1f;

    public TextMeshProUGUI formulaText;
    
    [Header("Element Styling")]
    [SerializeField] private MaterialList materialsList;

    private sealed class ElementRenderGroup
    {
        public readonly List<Matrix4x4> Matrices = new();
        public readonly MaterialPropertyBlock MaterialProperties = new();
    }

    private readonly struct AtomRenderData
    {
        public AtomRenderData(string symbol, Vector3 position)
        {
            Symbol = symbol;
            Position = position;
        }

        public readonly string Symbol;
        public readonly Vector3 Position;
    }

    private readonly Dictionary<string, ElementRenderGroup> renderGroups = new();
    private readonly Dictionary<string, Material> generatedMaterials = new();
    private readonly Matrix4x4[] drawBuffer = new Matrix4x4[MaxInstancesPerDraw];

    private Mesh activeAtomMesh;
    private Mesh generatedAtomMesh;
    private bool _shouldRender;

    private bool ShouldRender
    {
        get => _shouldRender && !string.IsNullOrWhiteSpace(smiles);
        set => _shouldRender = value;
    }

    private void Awake()
    {
        activeAtomMesh = GetAtomMesh();
    }

    private void Start()
    {
        if (!activeAtomMesh)
        {
            Debug.LogError("Unable to create or locate an atom mesh.");
            return;
        }

        GenerateAndPrepareMolecule(smiles);
    }

    private void OnDestroy()
    {
        foreach (Material material in generatedMaterials.Values)
        {
            if (material == null)
                continue;

            if (Application.isPlaying)
                Destroy(material);
            else
                DestroyImmediate(material);
        }

        generatedMaterials.Clear();

        if (generatedAtomMesh != null)
        {
            if (Application.isPlaying)
                Destroy(generatedAtomMesh);
            else
                DestroyImmediate(generatedAtomMesh);
        }
    }

    private void OnValidate() => coordinateScale = Mathf.Max(0.01f, coordinateScale);

    private void Update()
    {
        if (!ShouldRender)
            return;

        Mesh mesh = atomMesh ? atomMesh : activeAtomMesh;
        if (!mesh)
        {
            Debug.Log("No atom mesh found.");
            return;
        }

        foreach (KeyValuePair<string, ElementRenderGroup> pair in renderGroups)
        {
            ElementRenderGroup group = pair.Value;
            if (group.Matrices.Count == 0)
                continue;

            Material material = materialsList.GetMaterial(pair.Key);
            if (!material)
                continue;

            DrawInstanced(mesh, material, group);
        }
    }

    public void GenerateAndPrepareMolecule(string smilesString)
    {
        if (smiles == smilesString)
            return;

        smiles = smilesString;

        renderGroups.Clear();
        ShouldRender = false;

        if (string.IsNullOrWhiteSpace(smiles))
            return;

        string trimmedSmiles = smiles.Trim();

        try
        {
            using (RWMol mol = RWMol.MolFromSmiles(trimmedSmiles))
            {
                if (mol == null)
                {
                    invalidSMILES.gameObject.SetActive(!string.IsNullOrEmpty(smilesString));
                    formulaText.gameObject.SetActive(false);
                    return;
                }

                formulaText.gameObject.SetActive(true);
                formulaText.text = RDKFuncs.calcMolFormula(mol);

                invalidSMILES.gameObject.SetActive(false);
                RDKFuncs.addHs(mol);

                int embedResult = DistanceGeom.EmbedMolecule(mol);
                if (embedResult < 0)
                {
                    Debug.LogWarning("3D embedding failed. Falling back to 2D coordinates.");
                    mol.compute2DCoords();
                }
                else
                {
                    OptimizeMolecule(mol);
                }

                BuildRenderGroups(mol);
            }

            ShouldRender = renderGroups.Count > 0;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error embedding/rendering molecule: {e.Message}");
            ShouldRender = false;
        }
    }

    private void OptimizeMolecule(ROMol mol)
    {
        try
        {
            if (ForceField.UFFHasAllMoleculeParams(mol))
                ForceField.UFFOptimizeMolecule(mol);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"UFF optimization skipped: {e.Message}");
        }
    }

    private void BuildRenderGroups(ROMol mol)
    {
        List<AtomRenderData> atoms = new List<AtomRenderData>();
        Vector3 center = Vector3.zero;

        using (Conformer conformer = mol.getConformer())
        {
            uint atomCount = mol.getNumAtoms();
            for (uint i = 0; i < atomCount; i++)
            {
                using (Atom atom = mol.getAtomWithIdx(i))
                using (Point3D point = conformer.getAtomPos(i))
                {
                    string symbol = atom.getSymbol();
                    Vector3 position = new Vector3((float)point.x, (float)point.y, (float)point.z);

                    atoms.Add(new AtomRenderData(symbol, position));
                    center += position;
                }
            }
        }

        if (atoms.Count == 0)
            return;

        Vector3 offset = centerMolecule ? center / atoms.Count : Vector3.zero;
        foreach (AtomRenderData atom in atoms)
        {
            float scaleFactor = GetAtomScale(atom.Symbol) * atomScale;
            Vector3 position = (atom.Position - offset) * coordinateScale;
            Vector3 scale = Vector3.one * scaleFactor;
            Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, scale);

            if (!renderGroups.TryGetValue(atom.Symbol, out ElementRenderGroup group))
            {
                group = new ElementRenderGroup();
                renderGroups.Add(atom.Symbol, group);
            }

            group.Matrices.Add(matrix);
        }
    }

    private void DrawInstanced(Mesh mesh, Material material, ElementRenderGroup group)
    {
        int sourceIndex = 0;
        int remaining = group.Matrices.Count;
        var rootMatrix = RootMatrix;

        while (remaining > 0)
        {
            int count = Mathf.Min(MaxInstancesPerDraw, remaining);
            for (int i = 0; i < count; i++)
                drawBuffer[i] = rootMatrix * group.Matrices[sourceIndex + i];

            Graphics.DrawMeshInstanced(mesh, 0, material, drawBuffer, count, group.MaterialProperties);

            sourceIndex += count;
            remaining -= count;
        }
    }

    private Mesh GetAtomMesh()
    {
        if (atomMesh)
            return atomMesh;

        if (generatedAtomMesh)
            return generatedAtomMesh;

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Generated Atom Mesh Source";
        sphere.hideFlags = HideFlags.DontSave;
        sphere.SetActive(false);

        MeshFilter meshFilter = sphere.GetComponent<MeshFilter>();
        if (meshFilter && meshFilter.sharedMesh)
        {
            generatedAtomMesh = Instantiate(meshFilter.sharedMesh);
            generatedAtomMesh.name = "Generated Atom Sphere Mesh";
        }

        if (Application.isPlaying)
            Destroy(sphere);
        else
            DestroyImmediate(sphere);

        return generatedAtomMesh;
    }

    public void SetAtomScale(float newScale)
    {
        atomScale = newScale;
        GenerateAndPrepareMolecule(smiles);
    }

    private static float GetAtomScale(string symbol)
    {
        return symbol switch
        {
            "H" => 0.35f,
            "C" => 0.70f,
            "N" => 0.65f,
            "O" => 0.62f,
            "F" => 0.58f,
            "P" => 0.85f,
            "S" => 0.85f,
            "Cl" => 0.80f,
            "Br" => 0.88f,
            "I" => 0.95f,
            _ => 0.70f
        };
    }
}
