@echo on

git subtree split --prefix=Packages/com.capua.a2wtoolbox --branch upm
git remote rm upm
git remote add upm https://gitee.com/smallcorpse_rui/A2WToolbox.git
git push -f upm upm:main

pause