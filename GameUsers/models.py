from django.db import models
from django.contrib.auth.models import User
from django.utils import timezone
 
class GameUserMap(models.Model):
    user = models.ForeignKey(User, on_delete=models.CASCADE)
    created_on = models.DateTimeField(
        default=timezone.now
    )
    game = models.CharField(default='', max_length=200)
 
    def __str__(self):
        return self.game