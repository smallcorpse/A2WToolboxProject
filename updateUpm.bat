@echo on

git subtree split --prefix=Packages/com.capua.a2wtoolbox --branch upm
git pull upm main --allow-unrelated-histories

pause