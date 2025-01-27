using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TrialBlockVis : MonoBehaviour
{
    // Reference to the TextMeshProUGUI components
    public TextMeshProUGUI trialText;
    public TextMeshProUGUI blockText;

    // Reference to the DataOutputAndConfig script
    public DataOutputAndConfig dataSource;

    // Update is called once per frame
    void Update()
    {
        if (dataSource != null)
        {
            // Update the text elements
            trialText.text = "Trial: " + (dataSource.current_trial - 1);
            blockText.text = "Block: " + dataSource.current_block;
        }
    }
}
