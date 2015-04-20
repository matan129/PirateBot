#!/bin/bash

REPO_PATH=git@github.com:matan129/PirateBot.git
GH_REF=github.com/matan129/PirateBot.git
CONFIG_PATH=better-bots/Britbot/Britbot
HTML_PATH=better-bots/Britbot/Britbot/html
COMMIT_USER="Doxygen builder at TravisCi"
COMMIT_EMAIL="doxy@gen.com"
CHANGESET=$(git rev-parse --verify HEAD)

rm -rf ${HTML_PATH}
mkdir -p ${HTML_PATH}


git clone -b gh-pages "${REPO_PATH}" --single-branch ${HTML_PATH}
cd ${HTML_PATH}
git rm -rf .
cd -

cd ${CONIG_PATH}
doxygen BotDoc
cd html

git add .
git config user.name "${COMMIT_USER}"
git config user.email "${COMMIT_EMAIL}"
git commit -m "Automated documentation build for changeset ${CHANGESET}."
git push --force "https://${GH_TOKEN}@${GH_REF}" 