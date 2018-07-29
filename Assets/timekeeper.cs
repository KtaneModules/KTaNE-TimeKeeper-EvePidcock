using System.Linq;
using KMHelper;
using System;
using Random = UnityEngine.Random;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections;

public class timekeeper : MonoBehaviour {

    private static int _moduleIdCounter = 1;
    public KMAudio newAudio;
    public KMBombModule module;
    public KMBombInfo info;
    private int _moduleId = 0;
    private bool _isSolved = false, _lightsOn = false;

    private Color redC = new Color(255, 0, 0), greenC = new Color(0, 255, 0), blueC = new Color(0, 0, 255), yellowC = new Color(255, 255, 0), whiteC = new Color(255, 255, 255);

    public KMSelectable LEDOne;
    public KMSelectable LEDTwo;
    public KMSelectable LEDThree;
    private KMSelectable[] LEDs;
    public TextMesh displayedNumberText, colorblindTextOne, colorblindTextTwo, colorblindTextThree, colorblindTextScreen;
    private int displayedNumber;

    public GameObject colorblindObj;

    public MeshRenderer LEDOneMesh;
    public MeshRenderer LEDTwoMesh;
    public MeshRenderer LEDThreeMesh;

    private int LEDOneColor;
    private int LEDTwoColor;
    private int LEDThreeColor;
    private int displayedTextColor;

    private int red = 0;
    private int blue = 1;
    private int yellow = 2;
    private int green = 3;
    private int white = 4;
    private int black = 5;

    public Material[] colors = new Material[6]; //0=red,1=blue,2=yellow,3=green,4=white,5=black
    public String[] colorNames = new String[6]; //To make debug messages easier
    private Color[] textColors = new Color[5];  //0=red,1=blue,2=yellow,3=green,4=white

    private int month;

    private int correctLEDIndex;
    private int correctTime = 0;

    private bool end = false;

    char[] letters;
    int[] letterIndexes;
    int[] numbers;
    int batteryCount;
    int batteryHolderCount;
    int portPlateCount;
    int portCount;
    int litCount;
    int unlitCount;

    bool TwitchZenMode; 

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        module.OnActivate += Activate;
    }

    private void Awake()
    {
        LEDOne.OnInteract += delegate
        {
            handleLEDPress(0);
            return false;
        };
        LEDTwo.OnInteract += delegate
        {
            handleLEDPress(1);
            return false;
        };
        LEDThree.OnInteract += delegate
        {
            handleLEDPress(2);
            return false;
        };
    }

    void handleLEDPress(int index)
    {
        int timeLeft = (int)info.GetTime();
        if (!TwitchZenMode)
        {
            while (correctTime - timeLeft > 2)
            {
                correctTime = (int)Math.Floor((double)(correctTime / 2));
            }
        } else
        {
            while (timeLeft - correctTime > 2)
            {
                correctTime *= 2;
            }
        }
        newAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, LEDs[index].transform);
        if (_isSolved || !_lightsOn) return;
        if (index == correctLEDIndex)
        {
            if (Math.Abs(timeLeft - correctTime) <= 2)
            {
                if (timeLeft > 10)
                {
                    newAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, LEDs[index].transform);
                } else
                {
                    module.HandleStrike();
                    Debug.LogFormat("[TimeKeeper #{0}] Remember, this module does not like to wait. Since you pressed the correct answer with less than ten seconds remaining, you recieved a strike before the module got solved.", _moduleId);
                }
                module.HandlePass();
            } else
            {
                Debug.LogFormat("[TimeKeeper #{0}] Correct LED pressed at wrong time. Expected time: {1}. Recieved time: {2}.", _moduleId, correctTime, timeLeft);
                Debug.LogFormat("[TimeKeeper #{0}] If you feel that this strike is an error, please don't hesitate to contact @AAces#0908 on discord with a copy of this log file so we can get this sorted out.", _moduleId);
                module.HandleStrike();
            }
        } else
        {
            Debug.LogFormat("[TimeKeeper #{0}] Incorrect LED pressed. Expected {1}. Recieved: {2}.", _moduleId, correctLEDIndex+1, index+1);
            Debug.LogFormat("[TimeKeeper #{0}] If you feel that this strike is an error, please don't hesitate to contact @AAces#0908 on discord with a copy of this log file so we can get this sorted out.", _moduleId);
            module.HandleStrike();
        }
    }

    void Activate()
    {
        Init();
        _lightsOn = true;
    }

    void Init()
    {
        correctLEDIndex = -1;
        LEDs = new KMSelectable[3] { LEDOne, LEDTwo, LEDThree };
        letters = info.GetSerialNumberLetters().ToArray();
        numbers = info.GetSerialNumberNumbers().ToArray();
        batteryCount = info.GetBatteryCount();
        batteryHolderCount = info.GetBatteryHolderCount();
        portPlateCount = info.GetPortPlateCount();
        portCount = info.GetPortCount();
        litCount = info.GetOnIndicators().Count();
        unlitCount = info.GetOffIndicators().Count();
        letterIndexes = new int[letters.Length];
        for(int i = 0; i<letterIndexes.Length; i++)
        {
            letterIndexes[i] = GetIndexInAlphabet(letters[i]);
        }
        displayedNumber = Random.Range(1, 51);
        string text = displayedNumber.ToString();
        if(displayedNumber < 10)
        {
            displayedNumberText.text = "0" + text;
        } else
        {
            displayedNumberText.text = text;
        }
        setupColors();
        month = DateTime.Today.Month;
        Debug.LogFormat("[TimeKeeper #{0}] Displayed Number: {5} in {4}. LED One color: {1}. LED Two color: {2}. LED Three color: {3}.", _moduleId, colorNames[LEDOneColor], colorNames[LEDTwoColor], colorNames[LEDThreeColor], colorNames[displayedTextColor], displayedNumber);
        getCorrectAnswer();
    }

    void setupTextColors()
    {
        textColors[0] = new Color(255, 0, 0);
        textColors[1] = new Color(0, 0, 255);
        textColors[2] = new Color(255, 255, 0);
        textColors[3] = new Color(0, 255, 0);
        textColors[4] = new Color(255, 255, 255);
    }

    void setupColors()
    {
        int rndColor = Random.Range(0, 5);
        setupTextColors();
        displayedNumberText.color = textColors[rndColor];
        displayedTextColor = rndColor;
        rndColor = Random.Range(0, 6);
        LEDOneMesh.material = colors[rndColor];
        LEDOneColor = rndColor;
        rndColor = Random.Range(0, 6);
        LEDTwoMesh.material = colors[rndColor];
        LEDTwoColor = rndColor;
        rndColor = Random.Range(0, 6);
        LEDThreeMesh.material = colors[rndColor];
        LEDThreeColor = rndColor;

        switch (LEDOneColor)//0=red,1=blue,2=yellow,3=green,4=white,5=black
        {
            case 0:
                colorblindTextOne.color = redC;
                colorblindTextOne.text = "R";
                break;
            case 1:
                colorblindTextOne.color = blueC;
                colorblindTextOne.text = "Blu";
                break;
            case 2:
                colorblindTextOne.color = yellowC;
                colorblindTextOne.text = "Y";
                break;
            case 3:
                colorblindTextOne.color = greenC;
                colorblindTextOne.text = "G";
                break;
            case 4:
                colorblindTextOne.color = whiteC;
                colorblindTextOne.text = "W";
                break;
            case 5:
                colorblindTextOne.color = whiteC;
                colorblindTextOne.text = "Bla";
                break;
        }
        switch (LEDTwoColor)//0=red,1=blue,2=yellow,3=green,4=white,5=black
        {
            case 0:
                colorblindTextTwo.color = redC;
                colorblindTextTwo.text = "R";
                break;
            case 1:
                colorblindTextTwo.color = blueC;
                colorblindTextTwo.text = "Blu";
                break;
            case 2:
                colorblindTextTwo.color = yellowC;
                colorblindTextTwo.text = "Y";
                break;
            case 3:
                colorblindTextTwo.color = greenC;
                colorblindTextTwo.text = "G";
                break;
            case 4:
                colorblindTextTwo.color = whiteC;
                colorblindTextTwo.text = "W";
                break;
            case 5:
                colorblindTextTwo.color = whiteC;
                colorblindTextTwo.text = "Bla";
                break;
        }
        switch (LEDThreeColor)//0=red,1=blue,2=yellow,3=green,4=white,5=black
        {
            case 0:
                colorblindTextThree.color = redC;
                colorblindTextThree.text = "R";
                break;
            case 1:
                colorblindTextThree.color = blueC;
                colorblindTextThree.text = "Blu";
                break;
            case 2:
                colorblindTextThree.color = yellowC;
                colorblindTextThree.text = "Y";
                break;
            case 3:
                colorblindTextThree.color = greenC;
                colorblindTextThree.text = "G";
                break;
            case 4:
                colorblindTextThree.color = whiteC;
                colorblindTextThree.text = "W";
                break;
            case 5:
                colorblindTextThree.color = whiteC;
                colorblindTextThree.text = "Bla";
                break;
        }
        switch (displayedTextColor)//0=red,1=blue,2=yellow,3=green,4=white
        {
            case 0:
                colorblindTextScreen.color = redC;
                colorblindTextScreen.text = "Red";
                break;
            case 1:
                colorblindTextScreen.color = blueC;
                colorblindTextScreen.text = "Blue";
                break;
            case 2:
                colorblindTextScreen.color = yellowC;
                colorblindTextScreen.text = "Yellow";
                break;
            case 3:
                colorblindTextScreen.color = greenC;
                colorblindTextScreen.text = "Green";
                break;
            case 4:
                colorblindTextScreen.color = whiteC;
                colorblindTextScreen.text = "White";
                break;
        }

        if (GetComponent<KMColorblindMode>().ColorblindModeActive)
        {
            colorblindObj.SetActive(true);
            Debug.LogFormat("[TimeKeeper #{0}] Colorblind mode enabled.", _moduleId);
        } else
        {
            colorblindObj.SetActive(false);
        }
    }

    void getCorrectAnswer()
    {
        for(int i=1; i<22; i++)
        {
            if (end) break;
            steps(i);
        }
        if(correctTime < 0)
        {
            correctTime *= -1;
            Debug.LogFormat("[TimeKeeper #{0}] Correct time less than 0, multiplying by -1.", _moduleId);
        }
        if(correctTime < 10)
        {
            correctTime += 13;
            Debug.LogFormat("[TimeKeeper #{0}] Correct time less than 10, adding 13.", _moduleId);
        }
        if(correctLEDIndex == -1)
        {
            getCorrectLED();
        }
        Debug.LogFormat("[TimeKeeper #{0}] Correct LED: {1}. Correct time (in seconds): {2}.", _moduleId, correctLEDIndex + 1, correctTime);
    }

    private bool rule9 = false;

    void steps(int step)
    {
        switch (step)
        {
            case 1:
                correctTime = displayedNumber;
                for(int i = 0; i < letterIndexes.Length; i++)
                {
                    correctTime += letterIndexes[i];
                }
                for(int n =0; n < numbers.Length; n++)
                {
                    correctTime -= numbers[n];
                }
                Debug.LogFormat("[TimeKeeper #{0}] Rule 1 used.", _moduleId);
                
                break;
            case 2:
                if(LEDOneColor == white)
                {
                    correctTime += 14;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 2 used.", _moduleId);
                }
                break;
            case 3:
                if(LEDTwoColor == displayedTextColor)
                {
                    correctTime += 22;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 3 = true.", _moduleId);
                } else
                {
                    correctTime += 13;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 3 = false.", _moduleId);
                }
                break;
            case 4:
                correctTime += (2 * portPlateCount);
                Debug.LogFormat("[TimeKeeper #{0}] Rule 4 added " + (2 * portPlateCount), _moduleId);
                if (info.IsPortPresent(KMBombInfoExtensions.KnownPortType.DVI))
                {
                    correctTime -= 9;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 4: DVI found.", _moduleId);
                }
                break;
            case 5:
                if(LEDOneColor == LEDTwoColor && LEDOneColor == LEDThreeColor)
                {
                    setCorrectLED(0);
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 5 used.", _moduleId);
                }
                break;
            case 6:
                if((displayedTextColor == red || displayedTextColor == green || displayedTextColor == blue) && (LEDOneColor != yellow && LEDTwoColor != yellow && LEDThreeColor != yellow))
                {
                    correctTime += displayedNumber;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 6 used.", _moduleId);
                }
                break;
            case 7:
                if (info.GetSolvableModuleNames().Count() > (batteryCount+batteryHolderCount))
                {
                    correctTime -= 18;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 7 used.", _moduleId);
                }
                break;
            case 8:
                if(correctTime % 2 == 0 && correctTime > 72)
                {
                    correctTime /= 2;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 8 used.", _moduleId);
                }
                break;
            case 9:
                if(LEDTwoColor == green || LEDTwoColor == black)
                {
                    setCorrectLED(1);
                    rule9 = true;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 9 used.", _moduleId);
                }
                break;
            case 10:
                if((correctTime % 23) < (2 * portCount))
                {
                    end = true;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 10 used.", _moduleId);
                }
                break;
            case 11:
                correctTime += month;
                Debug.LogFormat("[TimeKeeper #{0}] Rule 11 used.", _moduleId);
                break;
            case 12:
                if (displayedNumber > 23)
                {
                    correctTime += batteryHolderCount;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 12 = true.", _moduleId);
                } else
                {
                    correctTime *= batteryHolderCount;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 12 = false.", _moduleId);
                }
                break;
            case 13:
                correctTime += (2 * litCount);
                correctTime -= (3 * unlitCount);
                Debug.LogFormat("[TimeKeeper #{0}] Rule 13 added " + (2 * litCount) + " and subtracted " + (3 * unlitCount), _moduleId);
                break;
            case 14:
                if (LEDOneColor == displayedTextColor && LEDOneColor == LEDThreeColor && LEDOneColor != LEDTwoColor)
                {
                    setCorrectLED(2);
                    end = true;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 14 used.", _moduleId);
                }
                break;
            case 15:
                if (rule9)
                {
                    correctTime += 10;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 15 = true.", _moduleId);
                } else
                {
                    correctTime -= 19;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 15 = false.", _moduleId);
                }
                break;
            case 16:
                if(correctTime < 0)
                {
                    correctTime *= -2;
                    end = true;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 16 used.", _moduleId);
                }
                break;
            case 17:
                correctTime *= 3;
                Debug.LogFormat("[TimeKeeper #{0}] Rule 17 used.", _moduleId);
                break;
            case 18:
                if((colorNames[LEDOneColor].Length + colorNames[LEDTwoColor].Length + colorNames[LEDThreeColor].Length) > 13)
                {
                    correctTime += colorNames[displayedTextColor].Length;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 18 used.", _moduleId);
                }
                break;
            case 19:
                if(portPlateCount == 0)
                {
                    end = true;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 19 used.", _moduleId);
                }
                break;
            case 20:
                if (info.IsIndicatorPresent(KMBombInfoExtensions.KnownIndicatorLabel.FRK)){
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 20 FRK out used.", _moduleId);
                    break;
                }
                if(colorNames[LEDOneColor].Length > colorNames[LEDTwoColor].Length && colorNames[LEDOneColor].Length > colorNames[LEDThreeColor].Length)
                {
                    setCorrectLED(0);
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 20 LED 1 used.", _moduleId);
                } else if (colorNames[LEDTwoColor].Length > colorNames[LEDOneColor].Length && colorNames[LEDTwoColor].Length > colorNames[LEDThreeColor].Length)
                {
                    setCorrectLED(1);
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 20 LED 2 used.", _moduleId);
                }
                else if (colorNames[LEDThreeColor].Length > colorNames[LEDTwoColor].Length && colorNames[LEDThreeColor].Length > colorNames[LEDOneColor].Length)
                {
                    setCorrectLED(2);
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 20 LED 3 used.", _moduleId);
                } else
                {
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 20 Tie", _moduleId);
                }
                break;
            case 21:
                if(unlitCount == 0)
                {
                    correctTime *= 3;
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 21 no unlit.", _moduleId);
                    break;
                } 
                foreach(string label in info.GetOffIndicators())
                {
                    correctTime += (GetIndexInAlphabet(label[0]));
                    Debug.LogFormat("[TimeKeeper #{0}] Rule 21: added " + label, _moduleId);
                }
                break;
        }
    }

    void getCorrectLED()
    {
        if(correctTime < 100)
        {
            correctLEDIndex = 0;
        } else if (displayedTextColor == green && LEDOneColor != green)
        {
            correctLEDIndex = 2;
        } else if(LEDOneColor != LEDTwoColor && LEDOneColor != LEDThreeColor && LEDOneColor != displayedTextColor && LEDTwoColor != LEDThreeColor && LEDTwoColor != displayedTextColor && LEDThreeColor != displayedTextColor)
        {
            correctLEDIndex = 0;
        } else if (info.IsPortPresent(KMBombInfoExtensions.KnownPortType.Parallel))
        {
            correctLEDIndex = 1;
        } else
        {
            correctLEDIndex = 2;
        }
    }

    private static int GetIndexInAlphabet(char value)
    {
        switch (value)
        {
            case 'A':
                return 1;
            case 'B':
                return 2;
            case 'C':
                return 3;
            case 'D':
                return 4;
            case 'E':
                return 5;
            case 'F':
                return 6;
            case 'G':
                return 7;
            case 'H':
                return 8;
            case 'I':
                return 9;
            case 'J':
                return 10;
            case 'K':
                return 11;
            case 'L':
                return 12;
            case 'M':
                return 13;
            case 'N':
                return 14;
            case 'O':
                return 15;
            case 'P':
                return 16;
            case 'Q':
                return 17;
            case 'R':
                return 18;
            case 'S':
                return 19;
            case 'T':
                return 20;
            case 'U':
                return 21;
            case 'V':
                return 22;
            case 'W':
                return 23;
            case 'X':
                return 24;
            case 'Y':
                return 25;
            case 'Z':
                return 26;
            default:
                return 0;
        }
    }

    void setCorrectLED(int index)
    {
        if (correctLEDIndex == -1)
        {
            correctLEDIndex = index;
        }
    }

#pragma warning disable 414

    private string TwitchHelpMessage = "Press the second LED at 3m14s with !{0} press 2 at 3:14. NOTE: To submit a time of 43 seconds, use '0:43', not just '43'.";

#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string input)
    {
        Regex rgx = new Regex(@"^press [123] (at|on) [0-5]?[0-9]:[0-5][0-9]$");
        if (rgx.IsMatch(input))
        {
            string[] split = input.ToLowerInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] time = split[3].ToLowerInvariant().Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            int led = Int32.Parse(split[1]) - 1;
            int seconds = (60 * Int32.Parse(time[0])) + (Int32.Parse(time[1]));
            yield return null;

            if (!TwitchZenMode)
            {
                if (Mathf.FloorToInt(info.GetTime()) < seconds)
                {
                    yield break;
                }
            } else
            {
                if(Mathf.FloorToInt(info.GetTime()) > seconds)
                {
                    yield break;
                }
            }
            while (Mathf.FloorToInt(info.GetTime()) != seconds) yield return "trycancel LED wasn't pressed due to request to cancel.";

            handleLEDPress(led);
        }
    }
}
