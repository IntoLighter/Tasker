var nameInput = $('#task_Name');
var descriptionInput = $('#task_Description');
var listOfExecutorsInput = $('#task_ListOfExecutors');
var statusSelect = $('#task_Status');
var dateOfRegistrationInput = $('#task_DateOfRegistration');
var dateOfCompleteInput = $('#task_DateOfComplete');
var plannedComplexityInput = $('#task_PlannedComplexity');
var actualExecutionTimeInput = $('#task_ActualExecutionTime');
var inputsArray = [nameInput, descriptionInput, listOfExecutorsInput,
    dateOfRegistrationInput, dateOfCompleteInput,
    plannedComplexityInput, actualExecutionTimeInput];
var taskForm = $('#task-form');
var taskTreeFocusedElement;
var currentErrorAlert;

function showErrorAlert(errorName) {
    if (currentErrorAlert !== undefined) {
        currentErrorAlert.remove();
    }
    $.get("../html/alert.html", function (html) {
        $.ajax({
            url: "api/ErrorMessage/".concat(errorName),
            cache: false,
            success: function (msg) {
                html = $(html).prepend(msg).get(0);
                taskForm.after(html);
                currentErrorAlert = $('.alert');
                setTimeout(function () {
                    currentErrorAlert.remove();
                }, 4000);
            }
        });
    });
}

function traverseChildren(node, callback) {
    var queue = [];
    queue.push(node);
    while (queue.length !== 0) {
        node = queue.shift();
        callback(node);
        $.each(node.childrenTasks, function (_, val) {
            queue.push(val);
        });
    }
}

$('.task-forest-node-name').on('click', function (event) {
    event.stopPropagation();
    if (taskTreeFocusedElement !== undefined) {
        taskTreeFocusedElement.removeClass('task-forest-node-focused');
        $('.task-tree').find('span').remove();
    }
    taskTreeFocusedElement = $(this).parent();
    taskTreeFocusedElement.addClass('task-forest-node-focused');
    $.getJSON("Home/GetTask", {
        id: taskTreeFocusedElement.attr('task-id')
    }, function (resp) {
        nameInput.val(resp.name);
        descriptionInput.val(resp.description);
        listOfExecutorsInput.val(resp.listOfExecutors);
        statusSelect.children('[selected]').removeAttr('selected');
        statusSelect.children().eq(resp.status).attr('selected', 'selected');
        dateOfRegistrationInput.val(resp.dateOfRegistration);
        dateOfCompleteInput.val(resp.dateOfComplete);
        plannedComplexityInput.val(resp.plannedComplexity);
        actualExecutionTimeInput.val(resp.actualExecutionTime);
        traverseChildren(resp, function (node) {
            taskTreeFocusedElement.parent().find("[task-id=".concat(node.id, "]"))
                .append($('<span>', {
                    'text': "(".concat(node.actualExecutionTime, ")"),
                    'class': 'text-light'
                }));
        });
    });
});
$('#CreateSubtask').on('click', function (event) {
    if (taskTreeFocusedElement === undefined) {
        showErrorAlert("TaskNotSelected");
        return;
    }
    taskForm.prepend($('<input>', {
        'id': 'ParentId',
        'name': 'ParentId',
        'type': 'hidden',
        'value': taskTreeFocusedElement.attr('task-id')
    }));
});
$('#SaveTask').on('click', function (event) {
    if (taskTreeFocusedElement === undefined) {
        showErrorAlert("TaskNotSelected");
        return;
    }
    taskForm.append($('<input>', {
        'id': 'task_Id',
        'name': 'task.Id',
        'type': 'hidden',
        'value': taskTreeFocusedElement.attr('task-id')
    }));
});
$('#DeleteTask').on('click', function (event) {
    if (taskTreeFocusedElement === undefined) {
        showErrorAlert("TaskNotSelected");
        return;
    }
    $.post("Home/Delete", {
        id: taskTreeFocusedElement.attr('task-id')
    }, function (event) {
        location.reload();
    }).fail(function (resp) {
        showErrorAlert("TaskNotLeaf");

    });
});
$('#ClearData').on('click', function (event) {
    $.each(inputsArray, function (_, val) {
        val.val('');
        val.parent().children('span').children().remove();
    });
    statusSelect.children('[selected]').removeAttr('selected');
    $('.task-tree').find('span').remove();
    if (taskTreeFocusedElement !== undefined) {
        taskTreeFocusedElement.removeClass('task-forest-node-focused');
        taskTreeFocusedElement = undefined;
    }
});
