using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class SortingScript : MonoBehaviour
{

    static public void CalculatePositions(List<Racer> array)
    {
        int finished = 0;
        List<Racer> sortedArray = new List<Racer>();

        for (int i = 0; i < array.Count; i++)
        {
            if (!array[i].finished)
                sortedArray.Add(array[i]);
            else
                finished += 1;
        }

        QuickSort(sortedArray, 0, sortedArray.Count - 1);

        for (int i = 0; i < sortedArray.Count; i++)
        {
            sortedArray[i].position = finished + i;
        }

        //Check if two racers have the same position
        List<int> collectedPositions = new List<int>();
        for (int i = 0; i < array.Count; i++)
        {
            if (!collectedPositions.Contains(array[i].position))
            {
                collectedPositions.Add(array[i].position);
            }
            else
            {
                throw new Exception("Same position appears twice!!!");
            }
        }

    }

    static private void QuickSort(List<Racer> array, int left, int right)
    {
        if (right - left >= 1)
        {
            int pivot = (right + left) / 2, leftCheck = left, rightCheck = right;

            while (leftCheck < rightCheck)
            {
                while (array[leftCheck].totalDistance > array[pivot].totalDistance ||
                    (array[leftCheck].totalDistance == array[pivot].totalDistance && array[leftCheck].currentDistance < array[pivot].currentDistance))
                    leftCheck += 1;

                while (array[rightCheck].totalDistance < array[pivot].totalDistance ||
                    (array[rightCheck].totalDistance == array[pivot].totalDistance && array[rightCheck].currentDistance > array[pivot].currentDistance))
                    rightCheck -= 1;

                if (leftCheck < rightCheck)
                {
                    if (leftCheck == pivot)
                    {
                        pivot = rightCheck;
                    }
                    else if (rightCheck == pivot)
                    {
                        pivot = leftCheck;
                    }

                    Swap(array, leftCheck, rightCheck);

                    if (leftCheck != pivot)
                        leftCheck += 1;

                    if (rightCheck != pivot)
                        rightCheck -= 1;

                }


            }

            QuickSort(array, left, pivot - 1);
            QuickSort(array, pivot + 1, right);

        }

        return;
    }

    static public List<DisplayRacer> CalculatePoints(List<DisplayRacer> toSort)
    {
        bool sorted = false;
        int endInt = 0;

        List<DisplayRacer> array = new List<DisplayRacer>(toSort);

        while (!sorted)
        {
            sorted = true;

            for (int i = 1; i < array.Count - endInt; i++)
            {
                if (array[i - 1].points < array[i].points)
                {
                    DisplayRacer holder = array[i - 1];
                    array[i - 1] = array[i];
                    array[i] = holder;
                    sorted = false;
                }
            }

            endInt++;
        }

        return array;

    }

    static public List<Racer> CalculatePoints(List<Racer> toSort)
    {
        bool sorted = false;
        int endInt = 0;

        List<Racer> array = new List<Racer>(toSort);

        while (!sorted)
        {
            sorted = true;

            for (int i = 1; i < array.Count - endInt; i++)
            {
                if (array[i - 1].points < array[i].points)
                {
                    Racer holder = array[i - 1];
                    array[i - 1] = array[i];
                    array[i] = holder;
                    sorted = false;
                }
            }

            endInt++;
        }

        for(int i = 0; i < array.Count; i++)
        {
            array[i].overallPosition = i;
        }

        return array;

    }


    static private void Swap(List<Racer> array, int a, int b)
    {
        Racer holder = array[a];

        array[a] = array[b];
        array[b] = holder;

        return;
    }
}
