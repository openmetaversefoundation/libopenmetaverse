/* SLProxy */

#import <Cocoa/Cocoa.h>

#import "Controller.h"

@interface SLProxy : NSObject
{
	NSTask *task;
	NSURL *loginURL;
}

- (SLProxy *)init;
- (void)dealloc;
- (NSURL *)loginURL;

@end
