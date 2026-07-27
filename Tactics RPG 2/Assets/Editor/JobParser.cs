using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;

public static class JobParser 
{
	[MenuItem("Pre Production/Parse Jobs")]
	public static void Parse()
	{
		CreateDirectories ();
		ParseStartingStats ();
		ParseGrowthStats ();
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	static void CreateDirectories ()
	{
		if (!AssetDatabase.IsValidFolder("Assets/Resources/Jobs"))
			AssetDatabase.CreateFolder("Assets/Resources", "Jobs");
	}

	static void ParseStartingStats ()
	{
		string readPath = string.Format("{0}/Settings/JobStartingStats.csv", Application.dataPath);
		string[] readText = File.ReadAllLines(readPath);
		for (int i = 1; i < readText.Length; ++i)
			PartsStartingStats(readText[i]);
	}

	static void PartsStartingStats (string line)
	{
		string[] elements = line.Split(',');
		GameObject obj = GetOrCreate(elements[0]);
		Job job = obj.GetComponent<Job>();
		EnsureStatArrays(job);
		for (int i = 0; i < Job.statOrder.Length; ++i)
			job.baseStats[i] = ReadStartingStat(elements, i);

<<<<<<< Updated upstream
		StatModifierFeature evade = GetFeature (obj, StatTypes.EVD);
		evade.amount = Convert.ToInt32(elements[8]);

		StatModifierFeature res = GetFeature (obj, StatTypes.RES);
		res.amount = Convert.ToInt32(elements[9]);

		StatModifierFeature move = GetFeature (obj, StatTypes.MOV);
		move.amount = Convert.ToInt32(elements[10]);

		StatModifierFeature jump = GetFeature (obj, StatTypes.JMP);
		jump.amount = Convert.ToInt32(elements[11]);
=======

		// In this project, every normal job stat lives in Job.baseStats / Job.growStats.
		// Older versions of the parser also added StatModifierFeature components for
		// SKL, FRT, LCK, MOV, and JMP. Those features are redundant with baseStats
		// and can subtract/double stats during future job changes, so remove them.
		RemoveGeneratedJobStatModifierFeatures(obj);
>>>>>>> Stashed changes
	}

	static void ParseGrowthStats ()
	{
		string readPath = string.Format("{0}/Settings/JobGrowthStats.csv", Application.dataPath);
		string[] readText = File.ReadAllLines(readPath);
		for (int i = 1; i < readText.Length; ++i)
			ParseGrowthStats(readText[i]);
	}

	static void ParseGrowthStats (string line)
	{
		string[] elements = line.Split(',');
		GameObject obj = GetOrCreate(elements[0]);
		Job job = obj.GetComponent<Job>();
		EnsureStatArrays(job);
		int count = Mathf.Min(elements.Length - 1, Job.statOrder.Length);
		for (int i = 0; i < count; ++i)
			job.growStats[i] = Convert.ToSingle(elements[i + 1]);

		int fortitudeIndex = IndexOf(StatTypes.FRT);
		if (fortitudeIndex >= 0 && elements.Length <= fortitudeIndex + 1)
		{
			int legacyLuckIndex = IndexOf(StatTypes.LCK);
			if (legacyLuckIndex >= 0 && elements.Length > legacyLuckIndex + 1)
				job.growStats[fortitudeIndex] = Convert.ToSingle(elements[legacyLuckIndex + 1]);
		}

	}


	static void EnsureStatArrays (Job job)
	{
		if (job.baseStats == null || job.baseStats.Length != Job.statOrder.Length)
		{
			int[] old = job.baseStats;
			job.baseStats = new int[Job.statOrder.Length];
			CopyStatsWithFortitudeFallback(old, job.baseStats);
		}

		if (job.growStats == null || job.growStats.Length != Job.statOrder.Length)
		{
			float[] old = job.growStats;
			job.growStats = new float[Job.statOrder.Length];
			CopyStatsWithFortitudeFallback(old, job.growStats);
		}
	}

	static void CopyStatsWithFortitudeFallback (int[] oldValues, int[] newValues)
	{
		if (oldValues == null)
			return;

		int count = Mathf.Min(oldValues.Length, newValues.Length);
		for (int i = 0; i < count; ++i)
			newValues[i] = oldValues[i];

		int fortitudeIndex = IndexOf(StatTypes.FRT);
		int luckIndex = IndexOf(StatTypes.LCK);
		if (fortitudeIndex >= 0 && luckIndex >= 0 && oldValues.Length > luckIndex)
			newValues[fortitudeIndex] = oldValues[luckIndex];
	}

	static void CopyStatsWithFortitudeFallback (float[] oldValues, float[] newValues)
	{
		if (oldValues == null)
			return;

		int count = Mathf.Min(oldValues.Length, newValues.Length);
		for (int i = 0; i < count; ++i)
			newValues[i] = oldValues[i];

		int fortitudeIndex = IndexOf(StatTypes.FRT);
		int luckIndex = IndexOf(StatTypes.LCK);
		if (fortitudeIndex >= 0 && luckIndex >= 0 && oldValues.Length > luckIndex)
			newValues[fortitudeIndex] = oldValues[luckIndex];
	}

	static int ReadStartingStat (string[] elements, int index)
	{
		if (index < 0)
			return 0;

		if (elements.Length > index + 1)
			return Convert.ToInt32(elements[index + 1]);

		if (Job.statOrder[index] == StatTypes.FRT)
		{
			int luckIndex = IndexOf(StatTypes.LCK);
			if (luckIndex >= 0 && elements.Length > luckIndex + 1)
				return Convert.ToInt32(elements[luckIndex + 1]);
		}

		return 0;
	}

	static int IndexOf (StatTypes type)
	{
		for (int i = 0; i < Job.statOrder.Length; ++i)
		{
			if (Job.statOrder[i] == type)
				return i;
		}
		return -1;
	}

	[MenuItem("Pre Production/Cleanup Job Stat Modifier Features")]
	public static void CleanupJobStatModifierFeatures ()
	{
		string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Resources/Jobs" });
		for (int i = 0; i < guids.Length; ++i)
		{
			string path = AssetDatabase.GUIDToAssetPath(guids[i]);
			GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (obj == null)
				continue;

			RemoveGeneratedJobStatModifierFeatures(obj);
			EditorUtility.SetDirty(obj);
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	static void RemoveGeneratedJobStatModifierFeatures (GameObject obj)
	{
		StatModifierFeature[] modifiers = obj.GetComponents<StatModifierFeature>();
		for (int i = modifiers.Length - 1; i >= 0; --i)
		{
			if (!IsGeneratedJobStatModifier(modifiers[i].type))
				continue;

			UnityEngine.Object.DestroyImmediate(modifiers[i], true);
		}
	}

	static bool IsGeneratedJobStatModifier (StatTypes type)
	{
		switch (type)
		{
		case StatTypes.SKL:
		case StatTypes.FRT:
		case StatTypes.LCK:
		case StatTypes.MOV:
		case StatTypes.JMP:
			return true;
		default:
			return false;
		}
	}

	static GameObject GetOrCreate (string jobName)
	{
		string fullPath = string.Format("Assets/Resources/Jobs/{0}.prefab", jobName);
		GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
		if (obj == null)
			obj = Create(fullPath);
		return obj;
	}

	static GameObject Create (string fullPath)
	{
		GameObject instance = new GameObject ("temp");
		instance.AddComponent<Job>();
		GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, fullPath);
		GameObject.DestroyImmediate(instance);
		return prefab;
	}
}