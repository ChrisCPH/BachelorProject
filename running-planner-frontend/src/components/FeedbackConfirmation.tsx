interface FeedbackConfirmationProps {
    onConfirm: () => void;
    onCancel: () => void;
}

export function FeedbackConfirmation({
    onConfirm,
    onCancel,
}: FeedbackConfirmationProps) {
    return (
        <div className="modal" style={{
            position: 'fixed',
            top: '0',
            left: '0',
            right: '0',
            bottom: '0',
            backgroundColor: 'rgba(0,0,0,0.5)',
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            zIndex: 1000
        }}>
            <div className="modal-content bg-dark text-light p-4 rounded" style={{
                width: '400px',
                maxWidth: '90%'
            }}>
                <h5>Run Completed</h5>
                <p>Would you like to provide feedback on this run?</p>

                <div className="d-flex justify-content-end gap-2 mt-4">
                    <button
                        className="btn btn-primary"
                        onClick={onConfirm}
                    >
                        Yes
                    </button>
                    <button
                        className="btn btn-outline-light"
                        onClick={onCancel}
                    >
                        No
                    </button>
                </div>
            </div>
        </div>
    );
}