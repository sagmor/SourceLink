﻿#load "Assemblies.fsx"

open SourceLink
open Fake

let isTfsBuild = hasBuildParam "tfsBuild"
let getTfsBuild() = new TfsBuild(getBuildParam "tfsUri", getBuildParam "tfsUser", getBuildParam "tfsAgent", getBuildParam "tfsBuild")

type Microsoft.Build.Evaluation.Project with
    member x.Compiles : FileIncludes = {
        BaseDirectory = x.DirectoryPath
        Includes = x.ItemsCompile
        Excludes = [] }

let verifyGitChecksums (repo:GitRepo) files =
    let different = repo.VerifyChecksums files
    if different.Length <> 0 then
        let errMsg = sprintf "%d source files do not have matching checksums in the git repository" different.Length
        log errMsg
        for file in different do
            logfn "no checksum match found for %s" file
        failwith errMsg

let verifyPdbChecksums (p:VsProject) files =
    let missing = p.VerifyPdbChecksums files
    if missing.Count > 0 then
        let errMsg = sprintf "cannot find %d source files" missing.Count
        log errMsg
        for file, checksum in missing.KeyValues do
            logfn "cannot find %s with checksum of %s" file checksum
        failwith errMsg