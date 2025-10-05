using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// DAWG Node - represents a single node in the Directed Acyclic Word Graph
/// </summary>
[System.Serializable]
public class DAWGNode
{
    public Dictionary<char, DAWGNode> children = new Dictionary<char, DAWGNode>();
    public bool isEndOfWord = false;
    public int nodeId = -1; // For suffix compression identification
    
    public DAWGNode()
    {
        children = new Dictionary<char, DAWGNode>();
        isEndOfWord = false;
        nodeId = -1;
    }
    
    /// <summary>
    /// Get a signature string for this node (for suffix compression)
    /// FIXED: Include node hash to prevent isEndOfWord conflicts
    /// </summary>
    public string GetSignature()
    {
        var sortedKeys = children.Keys.OrderBy(k => k).ToArray();
        string childrenSignature = string.Join(",", sortedKeys);
        
        // CRITICAL FIX: Create separate signatures for isEndOfWord true/false
        // This prevents merging nodes that have different word-ending properties
        if (isEndOfWord)
        {
            return childrenSignature + ":WORD_END";
        }
        else if (childrenSignature.Length > 0)
        {
            return childrenSignature + ":HAS_CHILDREN";
        }
        else
        {
            return ":LEAF_NO_WORD";
        }
    }
}

/// <summary>
/// Pure Trie implementation for reliable word validation
/// </summary>
public class WordDictionary 
{
    public WordDictionary(TextAsset wordDictionaryAsset){
        this.wordDictionaryAsset = wordDictionaryAsset;
        InitializeDictionary();
    }
    private TextAsset wordDictionaryAsset;

    // Trie root node
    private DAWGNode root = new DAWGNode();
    
    // Node ID tracking for debugging
    private int nextNodeId = 0;
    

    /// <summary>
    /// Initialize Trie from text asset
    /// </summary>
    private void InitializeDictionary()
    {
        if (wordDictionaryAsset == null)
        {
            Debug.LogError("WordDictionary: wordDictionaryAsset is null!");
            return;
        }

        string[] words = wordDictionaryAsset.text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        // Build Trie first
        foreach (string word in words)
        {
            string cleanWord = word.Trim();
            if (!string.IsNullOrEmpty(cleanWord))
            {
                AddWordToTrie(cleanWord);
            }
        }
    }

    /// <summary>
    /// Add a word to the Trie structure (assumes input is already uppercase)
    /// </summary>
    private void AddWordToTrie(string word)
    {
        DAWGNode current = root;
        
        foreach (char c in word)
        {
            if (!current.children.ContainsKey(c))
            {
                current.children[c] = new DAWGNode();
                current.children[c].nodeId = nextNodeId++;
            }
            current = current.children[c];
        }
        
        current.isEndOfWord = true;
    }
    
    /// <summary>
    /// Expose the root of the trie for external traversal.
    /// </summary>
    public DAWGNode GetRootNode()
    {
        return root;
    }

    
    /// <summary>
    /// Internal word validation without debug logging (assumes input is already uppercase)
    /// </summary>
    private bool IsValidWordInternal(string word)
    {
        if (string.IsNullOrEmpty(word))
            return false;
            
        DAWGNode node = FindNode(word);
        return node != null && node.isEndOfWord;
    }

    // REMOVED: Compression methods - using pure Trie for stability

    /// <summary>
    /// Count total nodes in the DAWG (for debugging)
    /// </summary>
    private int CountNodes(DAWGNode node)
    {
        HashSet<DAWGNode> visited = new HashSet<DAWGNode>();
        return CountNodesRecursive(node, visited);
    }

    private int CountNodesRecursive(DAWGNode node, HashSet<DAWGNode> visited)
    {
        if (visited.Contains(node))
            return 0;
            
        visited.Add(node);
        int count = 1;
        
        foreach (var child in node.children.Values)
        {
            count += CountNodesRecursive(child, visited);
        }
        
        return count;
    }

    /// <summary>
    /// Check if a string is a valid word (assumes input is already uppercase)
    /// </summary>
    /// <param name="word">The word to check</param>
    /// <returns>True if it's a valid word</returns>
    public bool IsValidWord(string word)
    {
        if (string.IsNullOrEmpty(word))
            return false;
            
        DAWGNode node = FindNode(word);
        return node != null && node.isEndOfWord;
    }

    /// <summary>
    /// Check if a string is a valid prefix using Trie
    /// </summary>
    /// <param name="prefix">The prefix to check</param>
    /// <returns>True if it's a valid prefix</returns>
    public bool HasValidPrefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            return true; // Empty prefix is always valid
            
        DAWGNode node = FindNode(prefix);
        return node != null;
    }

    /// <summary>
    /// Find a node in the Trie for given string (assumes input is already uppercase)
    /// </summary>
    /// <param name="str">String to search for</param>
    /// <returns>Node if found, null otherwise</returns>
    private DAWGNode FindNode(string str)
    {
        DAWGNode current = root;
        
        foreach (char c in str)
        {
            if (!current.children.ContainsKey(c))
            {
                return null;
            }
            current = current.children[c];
        }
        
        return current;
    }

    /// <summary>
    /// Get all words that start with the given prefix using Trie
    /// </summary>
    /// <param name="prefix">The prefix to search for</param>
    /// <returns>List of words starting with the prefix</returns>
    public List<string> GetWordsWithPrefix(string prefix)
    {
        List<string> result = new List<string>();
        
        if (string.IsNullOrEmpty(prefix))
            return result;
            
        DAWGNode prefixNode = FindNode(prefix);
        if (prefixNode == null)
            return result;
            
        // Collect all words from this node (prefix is already uppercase)
        CollectWords(prefixNode, prefix, result);
        return result;
    }

    /// <summary>
    /// Recursively collect all words from a DAWG node
    /// </summary>
    private void CollectWords(DAWGNode node, string currentWord, List<string> result)
    {
        if (node.isEndOfWord)
        {
            result.Add(currentWord);
        }
        
        foreach (var kvp in node.children)
        {
            CollectWords(kvp.Value, currentWord + kvp.Key, result);
        }
    }

    /// <summary>
    /// Get the total number of words in the dictionary
    /// </summary>
    /// <returns>Number of words</returns>
    public int GetWordCount()
    {
        return CountWords(root);
    }

    /// <summary>
    /// Count total words in DAWG
    /// </summary>
    private int CountWords(DAWGNode node)
    {
        HashSet<DAWGNode> visited = new HashSet<DAWGNode>();
        return CountWordsRecursive(node, visited);
    }

    private int CountWordsRecursive(DAWGNode node, HashSet<DAWGNode> visited)
    {
        if (visited.Contains(node))
            return 0;
            
        visited.Add(node);
        int count = node.isEndOfWord ? 1 : 0;
        
        foreach (var child in node.children.Values)
        {
            count += CountWordsRecursive(child, visited);
        }
        
        return count;
    }

    /// <summary>
    /// Check if the dictionary is initialized
    /// </summary>
    /// <returns>True if dictionary is ready</returns>
    public bool IsInitialized()
    {
        return root.children.Count > 0;
    }

    /// <summary>
    /// Get possible next characters from current prefix
    /// </summary>
    /// <param name="prefix">Current prefix</param>
    /// <returns>List of possible next characters</returns>
    public List<char> GetPossibleNextCharacters(string prefix)
    {
        List<char> result = new List<char>();
        
        DAWGNode node = string.IsNullOrEmpty(prefix) ? root : FindNode(prefix);
        if (node != null)
        {
            result.AddRange(node.children.Keys);
        }
        
        return result;
    }
}
