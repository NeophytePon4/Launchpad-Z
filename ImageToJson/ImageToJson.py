
import cv2
import sys
import json
import os

class ToJson:

    def __init__(self):
        self.file = "LP.png"
    def printData(self):
        if self.img != None:
            for i in self.img:
                for j in i:
                    print(j)

    def getImageArray(self, img):
        for i in range(len(img)):
            for j in range(len(img[i])):
                img[i][j] = img[i][j][::-1]
        return img


    def saveToJson(self, name, imageData, type = "img"):
        prevData = json.load(open("LPImages.json", "r"))
        newData = prevData
        newData[name] = {"type" : type,
                            "data" : imageData.tolist()}
        print(newData)
        json.dump(newData, open("LPImages.json", "w"))

    def convertFromFolder(self, folder):
        print(os.listdir(folder))
        files = os.listdir(folder)
        for file in files:
            print(file)
            self.saveToJson(file[:-4], converter.getImageArray(cv2.imread("convert\\{}".format(file),1)))

converter = ToJson()
#converter.getImageArray()
#converter.saveToJson("Peen of suck", converter.getImageArray())
print("This image converter will convert all png files in the convert folder\ninto the LPImages json file. Place all images to be converted in\nthe convert folder along with making sure the LPImages.json file\nis present in the same folder as this script.")
print("If you wish to convert all images in the convert folder press y, otherwise anykey will escape the program")
key = input(">>")
if key.lower() == "y":
    print("Converting...")
    converter.convertFromFolder("convert")
    print("Converted")
else:
    print("Exiting program...")