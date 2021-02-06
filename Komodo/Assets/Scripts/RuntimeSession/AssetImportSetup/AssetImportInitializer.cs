﻿// University of Illinois/NCSA
// Open Source License
// http://otm.illinois.edu/disclose-protect/illinois-open-source-license

// Copyright (c) 2020 Grainger Engineering Library Information Center.  All rights reserved.

// Developed by: IDEA Lab
//               Grainger Engineering Library Information Center - University of Illinois Urbana-Champaign
//               https://library.illinois.edu/enx

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal with
// the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to
// do so, subject to the following conditions:
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimers.
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimers in the documentation
//   and/or other materials provided with the distribution.
// * Neither the names of IDEA Lab, Grainger Engineering Library Information Center,
//   nor the names of its contributors may be used to endorse or promote products
//   derived from this Software without specific prior written permission.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// CONTRIBUTORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS WITH THE
// SOFTWARE.


using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// To Invoke our process of downloading and setting up imported objects to be used in session
/// </summary>
public class AssetImportInitializer : MonoBehaviour
{
    //text ui to dissplay progress of our download
    public Text progressDisplay;

    public AssetDownloaderAndLoader loader;

    //url asset list
    public AssetDataTemplate assetDataContainer;

    [Header("For Customizing our Asset Setup Process")]
    public AssetImportSetupSettings settings;

    //root object of url assets
    private GameObject parentOfImportedModels;

    private IEnumerator Start()
    {
        if (loader == null) {
            throw new System.Exception("Missing loader");
        }
        if (assetDataContainer == null)
            Debug.LogError("Missing import object list in AssetImportInitializer.cs", gameObject);

        if (progressDisplay == null)
            Debug.LogError("Missing import object ui text component in AssetImportInitializer.cs", gameObject);

        //create root parent in scene to contain all imported assets
        parentOfImportedModels = new GameObject("ImportedModels");
        parentOfImportedModels.transform.parent = transform;

        //Wait until all objects are finished loading
        yield return StartCoroutine(LoadAllGameObjectsFromURLs());

        //Set url download done state
        GameStateManager.Instance.isAssetImportFinished = true;

       // [Header("Flags for custom ImportProcess")]
    }

    public IEnumerator LoadAllGameObjectsFromURLs()
    {
        //wait for each loaded object to process
        for (int i = 0; i < assetDataContainer.assets.Count; i += 1)
        {
            var assetData = assetDataContainer.assets[i];
            VerifyAssetData(assetData);

            //download or load our asset
            yield return loader.GetFileFromURL(assetData, progressDisplay, i, gObject =>
            {
                //set up gameObject properties for our session and then set it as a child of our root object
                var assetData = assetDataContainer.assets[i];
                GameObject go = AssetImportSessionSetupUtility.SetupGameObject(i, assetData, gObject, settings ?? null);
                go.transform.SetParent(parentOfImportedModels.transform, true);
            });
        }
    }

    public void VerifyAssetData (AssetDataTemplate.AssetImportData data) {
        if (string.IsNullOrEmpty(data.name) || string.IsNullOrWhiteSpace(data.name)) {
            throw new System.Exception("Asset Data name cannot be empty.");
        }

        if (string.IsNullOrEmpty(data.url) || string.IsNullOrWhiteSpace(data.url)) {
            throw new System.Exception("Asset Data URL cannot be empty.");
        }
    }
}
