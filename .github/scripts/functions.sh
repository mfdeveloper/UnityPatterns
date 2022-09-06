#!/bin/bash

# TODO: Use this to get the package.json version
# USAGE: packageVersion "[PATH]/package.json"
packageVersion() {
    local PACKAGE_JSON_FILE=$1
    VERSION=""
    while read a b ; do 
        [ "$a" = '"version":' ] && { b="${b%\"*}" ; VERSION="${b#\"}" ; break ; }
    done < PACKAGE_JSON_FILE
    return $VERSION
}

githubActionsOutputs() {
    CURRENT_TAG=$(git describe --tags $(git rev-list --tags --max-count=1))
    COMMIT_MESSAGE=$(git log -1 --pretty=%B)
    echo ::set-output name=tag::$CURRENT_TAG
    echo ::set-output name=commit_message::$COMMIT_MESSAGE
}

copyPackagesContent() {
    shopt -s extglob dotglob
    cp -rvf "Packages/$PKG_NAME/." ./
    rm -rf ./Packages
}

commitAndPush() {
    # TODO: Get version from package.json using the function packageVersion() file
    [[ "$RELEASE_VERSION" =~ (.*[^0-9])([0-9]+)$ ]] && RELEASE_VERSION="${BASH_REMATCH[1]}$((${BASH_REMATCH[2]} + 1))";

    echo "New version: $RELEASE_VERSION"

    if [[ -d "Samples" ]]; then
    mv Samples Samples~
    rm -f Samples.meta
    fi
    if [[ -d "Documentation" ]]; then
    mv Documentation Documentation~
    rm -f Documentation.meta
    fi
    git config --global user.name 'github-bot'
    git config --global user.email 'github-bot@users.noreply.github.com'
    git add .
    git commit --allow-empty -am "$COMMIT_MESSAGE"

    echo $RELEASE_VERSION > VERSION.md~
    git add VERSION.md~
    git commit -am "fix: Samples => Samples~ and commit a new version: $RELEASE_VERSION"
    git push -f -u origin "$PKG_BRANCH"
}

run() {
    if [ $1 == "push" ]
    then
        commitAndPush
    elif [ $1 == "movePackagesFolder" ]
    then
        copyPackagesContent
    elif [ $1 == "githubActionsVariables" ]
    then
        githubActionsOutputs
    else
        echo "[ERROR] INVALID SCRIPT OPERATION"
        exit 1
    fi
}

run $1