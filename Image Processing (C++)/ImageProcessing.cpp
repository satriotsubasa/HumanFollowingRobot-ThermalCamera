//#define _CRT_SECURE_NO_WARNINGS
#define _SCL_SECURE_NO_WARNINGS

#include <iostream>
#include <thread>
#include "opencv2/opencv.hpp"
#include "opencv2/highgui/highgui.hpp"
#include "opencv2/core/core.hpp"
#include <Windows.h>
#include <stdio.h>
#include <sstream>
#include <stdlib.h>
#include <string>
#include <vector>
#include "SerialPort.h"
#include <fstream>

using namespace cv;
using namespace std;
using std::cout;
using std::endl;

char *port_name = "\\\\.\\COM9";
char incomingData[MAX_DATA_LENGTH];

int _countParse, _countParse2, _totalData;
int H_MIN = 0;
int H_MAX = 256;
int S_MIN = 0;
int S_MAX = 256;
int V_MIN = 0;
int V_MAX = 256;
int callibr = 0;
int flag_clrArea = 0;
int safety = 1;
int xx, yy;
int koordinatX = 0, luasBlob;

const int FRAME_WIDTH = 400;
const int FRAME_HEIGHT = 400;
const int MAX_NUM_OBJECTS = 50;
const int MIN_OBJECT_AREA = 15*15;
const int MAX_OBJECT_AREA = FRAME_HEIGHT * FRAME_WIDTH;
const string trackbarWindowName = "Trackbars";

float _data[63], _dataNorm[70], tempMin = 27, tempMax = 31;
double _area = 0;
double max_temp, min_temp;

vector<string> split(string s, string delimiter)
{
	size_t pos_start = 0, pos_end, delim_len = delimiter.length();
	string token;
	vector <string> res;

	while ((pos_end = s.find(delimiter, pos_start)) != string::npos)
	{
		token = s.substr(pos_start, pos_end - pos_start);
		pos_start = pos_end + delim_len;
		res.push_back(token);
	}

	res.push_back(s.substr(pos_start));
	return res;
}
void on_trackbar(int, void*)
{

}
string intToString(int number)
{
	std::stringstream ss;
	ss << number;
	return ss.str();
}
void createTrackbars()
{
	char TrackbarName[50];
	namedWindow(trackbarWindowName, 0);
	sprintf_s(TrackbarName, "H_MIN", H_MIN);
	sprintf_s(TrackbarName, "H_MAX", H_MAX);
	sprintf_s(TrackbarName, "S_MIN", S_MIN);
	sprintf_s(TrackbarName, "S_MAX", S_MAX);
	sprintf_s(TrackbarName, "V_MIN", V_MIN);
	sprintf_s(TrackbarName, "V_MAX", V_MAX);
	createTrackbar("H_MIN", trackbarWindowName, &H_MIN, H_MAX, on_trackbar);
	createTrackbar("H_MAX", trackbarWindowName, &H_MAX, H_MAX, on_trackbar);
	createTrackbar("S_MIN", trackbarWindowName, &S_MIN, S_MAX, on_trackbar);
	createTrackbar("S_MAX", trackbarWindowName, &S_MAX, S_MAX, on_trackbar);
	createTrackbar("V_MIN", trackbarWindowName, &V_MIN, V_MAX, on_trackbar);
	createTrackbar("V_MAX", trackbarWindowName, &V_MAX, V_MAX, on_trackbar);
}
void drawObject(int x, int y, Mat &frame)
{
	circle(frame, Point(x, y), 20, Scalar(255, 0, 0), 2);
	if (y - 25 > 0)
		line(frame, Point(x, y), Point(x, y - 25), Scalar(255, 0, 0), 2);
	else line(frame, Point(x, y), Point(x, 0), Scalar(255, 0, 0), 2);
	if (y + 25 < FRAME_HEIGHT)
		line(frame, Point(x, y), Point(x, y + 25), Scalar(255, 0, 0), 2);
	else line(frame, Point(x, y), Point(x, FRAME_HEIGHT), Scalar(255, 0, 0), 2);
	if (x - 25 > 0)
		line(frame, Point(x, y), Point(x - 25, y), Scalar(255, 0, 0), 2);
	else line(frame, Point(x, y), Point(0, y), Scalar(255, 0, 0), 2);
	if (x + 25 < FRAME_WIDTH)
		line(frame, Point(x, y), Point(x + 25, y), Scalar(255, 0, 0), 2);
	else line(frame, Point(x, y), Point(FRAME_WIDTH, y), Scalar(255, 0, 0), 2);
	koordinatX = x;
}
void morphOps(Mat &thresh)
{
	Mat erodeElement = getStructuringElement(MORPH_RECT, Size(3, 3));
	Mat dilateElement = getStructuringElement(MORPH_RECT, Size(8, 8));
	erode(thresh, thresh, erodeElement);
	erode(thresh, thresh, erodeElement);
	dilate(thresh, thresh, dilateElement);
	dilate(thresh, thresh, dilateElement);
}
void trackFilteredObject(int &x, int &y, Mat threshold, Mat &image)
{
	ofstream myfile;
	Mat temp;
	threshold.copyTo(temp);
	vector< vector<Point> > contours;
	vector<Vec4i> hierarchy;
	findContours(temp, contours, hierarchy, RETR_CCOMP, CHAIN_APPROX_SIMPLE);

	double refArea = 0;
	bool objectFound = false;

	if (hierarchy.size() > 0)
	{
		int numObjects = hierarchy.size();
		if (numObjects < MAX_NUM_OBJECTS)
		{
			for (int index = 0; index >= 0; index = hierarchy[index][0])
			{
				Moments moment = moments((cv::Mat)contours[index]);
				double area = moment.m00;
				_area = area;
				if (area > MIN_OBJECT_AREA && area < MAX_OBJECT_AREA && area > refArea)
				{
					x = moment.m10 / area;
					y = moment.m01 / area;
					objectFound = true;
					int nDummy = (int)(area * 1000);
					refArea = area;
				}
				else objectFound = false;
			}
			if (objectFound == true)
			{
				drawObject(x, y, image);
			}
			flag_clrArea = 0;
			myfile.open("data.txt");
			myfile << koordinatX;
			myfile << '#';
			myfile << _area;
			myfile.close();
			putText(image, "Pos: " + intToString(koordinatX), Point(0, 10), 2, 0.5, Scalar(0, 0, 0), 2);
			putText(image, "Area: " + to_string(_area), Point(0, 25), 2, 0.5, Scalar(0, 0, 0), 2);
			putText(image, "MaxTmp: " + to_string(max_temp), Point(0, 390), 2, 0.5, Scalar(0, 0, 0), 2);
			putText(image, "MinTmp: " + to_string(min_temp), Point(0, 375), 2, 0.5, Scalar(0, 0, 0), 2);
		}
		else
		{
			if (flag_clrArea > 11)
			{
				_area = 0;
				flag_clrArea = 12;
			}
			flag_clrArea++;
			myfile.open("data.txt");
			myfile << koordinatX;
			myfile << '#';
			myfile << _area;
			myfile.close();
			putText(image, "Pos : " + intToString(koordinatX), Point(0, 10), 2, 0.5, Scalar(0, 0, 0), 2);
			putText(image, "Area : " + to_string(_area), Point(0, 25), 2, 0.5, Scalar(0, 0, 0), 2);
			putText(image, "MaxTmp: " + to_string(max_temp), Point(0, 390), 2, 0.5, Scalar(0, 0, 0), 2);
			putText(image, "MinTmp: " + to_string(min_temp), Point(0, 375), 2, 0.5, Scalar(0, 0, 0), 2);
		}
		
	}
	else
	{
		if (flag_clrArea > 11)
		{
			_area = 0;
			flag_clrArea = 12;
		}
		flag_clrArea++;
		myfile.open("data.txt");
		myfile << koordinatX;
		myfile << '#';
		myfile << _area;
		myfile.close();
		putText(image, "Pos : " + intToString(koordinatX), Point(0, 10), 2, 0.5, Scalar(0, 0, 0), 2);
		putText(image, "Area : " + to_string(_area), Point(0, 25), 2, 0.5, Scalar(0, 0, 0), 2);
		putText(image, "MaxTmp: " + to_string(max_temp), Point(0, 390), 2, 0.5, Scalar(0, 0, 0), 2);
		putText(image, "MinTmp: " + to_string(min_temp), Point(0, 375), 2, 0.5, Scalar(0, 0, 0), 2);
	}
}

void ImgProc()
{
	bool trackObjects = true;
	bool useMorphOps = true;
	Mat image(Size(8, 8), CV_32FC1, _dataNorm);
	Mat HSV, thres, image64, imageRAW, beforeMorph;
	vector< vector< cv::Point > > contours; vector< cv::Vec4i > hierarcy;
	if (safety == 1)
	{
		xx = 0;
		yy = 0;
		createTrackbars();
		safety = 0;
	}
	flip(image, image, +1);
	imageRAW = image.clone();
	image.convertTo(image, CV_8UC1);
	applyColorMap(image, image, COLORMAP_JET);
	//image64 = image.clone();
	resize(image, image, Size(), 50.0, 50.0, INTER_CUBIC);
	cvtColor(image, HSV, COLOR_BGR2HSV);
	inRange(HSV, Scalar(H_MIN, S_MIN, V_MIN), Scalar(H_MAX, S_MAX, V_MAX), thres);
	//inRange(image, Scalar(H_MIN, S_MIN, V_MIN), Scalar(H_MAX, S_MAX, V_MAX), thres);
	cv::findContours(thres, contours, hierarcy, cv::RETR_LIST, cv::CHAIN_APPROX_SIMPLE);
	//printf("blob %d \n", contours.size());
	vector<int> sortIdx(contours.size());
	vector<float> areas(contours.size());
	for (int n = 0; n < (int)contours.size(); n++)
	{
		sortIdx[n] = n;
		areas[n] = contourArea(contours[n], false);
	}
	int idx;
	if (useMorphOps)
		morphOps(thres);

	if (trackObjects)
	{
		trackFilteredObject(xx, yy, thres, image);
		
	}
	//namedWindow("Original_Interpolate", WINDOW_AUTOSIZE);
	//namedWindow("Original", WINDOW_FREERATIO);
	//namedWindow("RAW_Image", WINDOW_FREERATIO);
	//imshow("RAW_Image", imageRAW);
	//imshow("Original", image64);
	imshow("Original_Interpolate", image);
	//moveWindow("Original_Interpolate", 120, 220);
	//imshow("HSV", HSV);
	imshow("Threshold", thres);
	//imshow("No Morph", beforeMorph);
	//inRange(image, (100, 100, 100), (255, 255, 255),mask);
	waitKey(0);
}

int main()
{
	HWND console = GetConsoleWindow();
	RECT ConsoleRect;
	GetWindowRect(console, &ConsoleRect);
	MoveWindow(console, 0, 0, 300, 300, TRUE);
	std::string _terima;
	SerialPort arduino(port_name);

	if (arduino.isConnected()) cout << "Connection Established" << endl;
	else cout << "ERROR, check port name";

	std::cout << std::fixed;
	koordinatX = 0;
	_area = 0;
	while (arduino.isConnected())
	{
	skip1:
		_countParse = 0;
		_countParse2 = 0;
		memset(incomingData, 0, MAX_DATA_LENGTH);
		int read_result = arduino.readSerialPort(incomingData, MAX_DATA_LENGTH);
		int pos1 = _terima.find("[");
		if (pos1 == std::string::npos)
		{
			_terima = _terima + incomingData;
			goto skip1;
		}
		int pos2 = _terima.find("]", pos1 + 1);
		if (pos2 == std::string::npos)
		{
			_terima = _terima + incomingData;
			goto skip1;
		}
		std::string _terima2 = _terima.substr((pos1+1), (pos2-pos1-1));
		std::string _delimiter = ",";
		_terima = "";
		vector<string> v = split(_terima2, _delimiter);
		for (auto i : v)
		{
			_data[_countParse2] = std::stof(i);
			_dataNorm[_countParse2] = ((_data[_countParse2] - tempMin) * (255 - 0)) / ((tempMax - tempMin) + 0);
			_countParse++;
			_countParse2++;
			if (_countParse == 8)
			{
				_countParse = 0;
			}
			if (_countParse2 == 64)
			{
				_countParse2 = 0;
				break;
			}
		}
		max_temp = *max_element(begin(_data), end(_data));
		min_temp = *min_element(begin(_data), end(_data));
		if (callibr < 10)
		{
			tempMin = *min_element(begin(_data), end(_data));
			tempMax = *max_element(begin(_data), end(_data));
			if (tempMax < 31)
			{
				tempMax = 31;
			}
			callibr++;
			cout << "Please Wait.. Callibration" << endl;
			if (callibr == 10) cout << "Callibration Finished" << endl;
			goto skip1;
		}
		thread t1(ImgProc);
		t1.detach();
	}
	waitKey(0);
}