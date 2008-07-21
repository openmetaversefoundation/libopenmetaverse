/* Controller */

#import <Cocoa/Cocoa.h>

#import "SLProxy.h"

NSTask *killtask;

@interface Controller : NSObject
{
}

+ (void)terminateOnFailure:(NSTask *)task;
+ (void)failBecause:(NSString *)reason;

@end
