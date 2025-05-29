using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoring : MonoBehaviour
{
    [SerializeField] AimShoot aimShoot;
    [SerializeField] GameObject scoreSpriteHolder;
    private Animator scoreAnimator;

    public bool targetNear; // 1 if target is full sized, 2 if target is far (sized down), which allows us to accurately score the small target
    [SerializeField] Sprite[] scoreSprites;


    // this is to make printing a little bit easier and is largely superfluous
    private enum rings : int
    {
        outta_here,
        inner_yellow = 0,
        mid_yellow = 1,
        outer_yellow = 2,
        inner_red = 3,
        outer_red = 4,
        inner_blue = 5,
        outer_blue = 6,
        inner_black = 7,
        outer_black = 8,
        inner_white = 9,
        outer_white = 10,
    }

    private void Start()
    {
        // grab sprites
        scoreSpriteHolder.SetActive(true);
        scoreAnimator = scoreSpriteHolder.GetComponent<Animator>();
    }

    public void scoreShot(Transform arrowLocal)
    { 
        Vector3 arrowRelativePos = Vector3.zero;
        arrowRelativePos = arrowLocal.position - aimShoot.featureTarget.transform.position;

        print("SCORING CALLED -> arrow local is: " + arrowLocal.position);

        float distance = calculateDistance(arrowRelativePos);
        int score;
        if (targetNear != true)
        {
            score = calculateFarScore(distance);
        } else
        {
            score = calculateScore(distance);
        }
        handleAnimation(score);
    }

    // will auto-pull the data from AimShoot to get the distance of the dart from the middle of the target
    public float calculateDistance(Vector3 arrowLocal)
    {
        Vector2 target2D = new Vector2(0f, 0f); // Now the origin
        Vector2 arrow2D = new Vector2(arrowLocal.x, arrowLocal.y);
        Debug.Log("<color=green>SCORING: the target is </color>" + target2D + "arrow2d: + " + arrow2D);
        // calculate the distance between the center of the target and the arrow position
        float radialError = Vector2.Distance(target2D, arrow2D);

        return radialError;
    }

    public int calculateFarScore(float distanceFromCenter)
    {
        // max score of 100, each ring from this is 10 points less
        int score = 100;
        // check if we're in the very center ring (less than .086), and if so return the max score
        if (distanceFromCenter <= .043)
        {
            score = 150;
            Debug.Log("<color=red>PERFECT SCORE~ THE DISTANCE FROM TARGET IS </color>" + score);
            return score;
        }

        // should first calculate the ring that we reside in
        int ring_num = (int)(distanceFromCenter / .0868) + 1;
        Debug.Log("<color=purple> OUR RING IS </color>" + ((rings)ring_num) + "   " + ring_num);

        score -= (ring_num - 1) * 10;
        // then clamp it, no negative numbers here
        // could also change this whole script to only get called when the target is hit, not the entire target block
        // in which case we wouldn't need this
        score = Mathf.Clamp(score, 0, 100);
        Debug.Log("<color=purple>SCORE FROM TARGET IS </color>" + score);
        return score;
    }

    // in real archery, the center ring is worth 15 points, the second ring is worth 10 points, then 1 less point for every additional ring
    // in our case we're starting at 150 and decreasing by 10 for no reasons other than the aesthetics of the scoring
    public int calculateScore(float distanceFromCenter)
    {
        // max score of 100, each ring from this is 10 points less
        int score = 100;
        // check if we're in the very center ring (less than .086), and if so return the max score
        if (distanceFromCenter <= .086)
        {
            score = 150;
            Debug.Log("<color=red>PERFECT SCORE~ THE DISTANCE FROM TARGET IS </color>" + score);
            return score;
        }

        // should first calculate the ring that we reside in
        int ring_num = (int) (distanceFromCenter / (.0868*2) + 1);
        Debug.Log("<color=green> OUR RING IS </color>" + ((rings)ring_num));

        score -= (ring_num-1)*10;
        // then clamp it, no negative numbers here
        // could also change this whole script to only get called when the target is hit, not the entire target block
        // in which case we wouldn't need this
        score = Mathf.Clamp(score,0,100);
        Debug.Log("<color=green>SCORE FROM TARGET IS </color>" + score);
        return score;
    }

    private void handleAnimation(int score)
    {
        // sorry
        // set the sprite depending on the score, but the animation will be the same
        if (score > 100) {
            scoreSpriteHolder.GetComponent<SpriteRenderer>().sprite = scoreSprites[0];
        } else if (score > 90)
        {
            scoreSpriteHolder.GetComponent<SpriteRenderer>().sprite = scoreSprites[1];
        } else if (score > 80)
        {
            scoreSpriteHolder.GetComponent<SpriteRenderer>().sprite = scoreSprites[2];
        } else if (score > 70)
        {
            scoreSpriteHolder.GetComponent<SpriteRenderer>().sprite = scoreSprites[3];
        } else if (score > 60)
        {
            scoreSpriteHolder.GetComponent<SpriteRenderer>().sprite = scoreSprites[4];
        } else if (score > 50)
        {
            scoreSpriteHolder.GetComponent<SpriteRenderer>().sprite = scoreSprites[5];
        } else if (score > 40)
        {
            scoreSpriteHolder.GetComponent<SpriteRenderer>().sprite = scoreSprites[6];
        } else if (score > 30)
        {
            scoreSpriteHolder.GetComponent<SpriteRenderer>().sprite = scoreSprites[7];
        } else if (score > 20)
        {
            scoreSpriteHolder.GetComponent<SpriteRenderer>().sprite = scoreSprites[8];
        } else if (score > 10)
        {
            scoreSpriteHolder.GetComponent<SpriteRenderer>().sprite = scoreSprites[9];
        } else if (score > 0)
        {
            scoreSpriteHolder.GetComponent<SpriteRenderer>().sprite = scoreSprites[10];
        } else // didn't actually get any points, don't play any animation
        {
            return;
        }

        scoreAnimator.Play("showScore");
    }

}

