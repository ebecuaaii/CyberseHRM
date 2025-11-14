-- Migration: Create LateRequest table
-- Date: 2025-11-06
-- Description: Add table for late arrival requests

CREATE TABLE IF NOT EXISTS laterequests (
    id SERIAL PRIMARY KEY,
    userid INTEGER NOT NULL,
    shiftid INTEGER NOT NULL,
    requestdate DATE NOT NULL,
    expectedarrivaltime TIME NOT NULL,
    reason TEXT,
    status VARCHAR(20) DEFAULT 'Pending',
    reviewedby INTEGER,
    reviewedat TIMESTAMP WITH TIME ZONE,
    createdat TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    
    CONSTRAINT laterequests_userid_fkey FOREIGN KEY (userid) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT laterequests_shiftid_fkey FOREIGN KEY (shiftid) 
        REFERENCES shifts(id) ON DELETE CASCADE,
    CONSTRAINT laterequests_reviewedby_fkey FOREIGN KEY (reviewedby) 
        REFERENCES users(id) ON DELETE SET NULL,
    CONSTRAINT laterequests_status_check CHECK (status IN ('Pending', 'Approved', 'Rejected', 'Cancelled'))
);

-- Create indexes for better query performance
CREATE INDEX idx_laterequests_userid ON laterequests(userid);
CREATE INDEX idx_laterequests_status ON laterequests(status);
CREATE INDEX idx_laterequests_requestdate ON laterequests(requestdate);

-- Add comments
COMMENT ON TABLE laterequests IS 'Stores employee late arrival requests';
COMMENT ON COLUMN laterequests.expectedarrivaltime IS 'Expected time of arrival when requesting to be late';
COMMENT ON COLUMN laterequests.status IS 'Request status: Pending, Approved, Rejected, Cancelled';
