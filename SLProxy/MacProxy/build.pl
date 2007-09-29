#!/usr/bin/perl -w

die "Usage: $0 \"Application Name\" path/to/executable.exe path/to/libraries.dll ...\n" unless @ARGV >= 2;

die "Please run MacProxy from within its own directory.\n" unless -d 'Launcher';

my ($appname, $exe, @libs) = @ARGV;

print "Creating application bundle...\n";

die "An application with that name already exists; aborting.\n" if -e "$appname.app";
system('cp', '-r', 'Launcher/build/Release/Launcher.app', "$appname.app")
and die "Failed; aborting.\n";

print "Embedding assemblies...\n";

system('cp', $exe, @libs, "$appname.app/Contents/Resources/Assemblies/")
and die "Failed; aborting.\n";

print "Writing metadata...\n";

open(my $ii, '<', 'Launcher/Info.plist')
or die "Failed to open Launcher/Info.plist; aborting.\n";
open(my $io, '>', "$appname.app/Contents/Info.plist")
or die "Failed to open $appname.app/Contents/Info.plist; aborting.\n";

my $id = $appname;
$id =~ s/[^a-z]//gi;
$id = 'x' unless length $id;
$exe =~ s!.*/!!;
while (<$ii>) {
	s/##NAME##/$appname/;
	s/##ID##/$id/;
	s/##EXE##/$exe/;
	print $io $_;
}

close $ii;
close $io;

print "Packaging bundle...\n";

system('tar', 'cjf', "$appname.tar.bz2", "$appname.app")
and die "Failed to create archive; aborting.\n";
system('rm', '-rf', "$appname.app")
and die "Failed to remove application bundle; aborting.\n";

system('ls', '-l', "$appname.tar.bz2")
and print "Done.\n";
